namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML.Elements;
    using Dashing.Extensions;

    internal class WhereClauseExpressionVisitor : BaseExpressionVisitor {
        private readonly ISqlDialect dialect;

        private readonly IConfiguration configuration;

        private FetchNode rootNode;

        private bool isChainedMemberAccess;

        private Expression chainedMemberAccessExpression;

        private bool getForeignKeyName;

        private string chainedColumnName;

        private Type chainedColumnType;

        private readonly Stack<string> chainedEntityNames;

        private bool isClosureConstantAccess;

        private string constantMemberAccessName;

        private int paramCounter;

        private bool doAppendValue;

        private bool doPrependValue;

        private string appendValue = string.Empty;

        private string prependValue = string.Empty;

        private bool insideSubQuery;

        private bool isTopOfBinary;

        private bool isInBinaryComparisonExpression;

        private int aliasCounter;

        public StringBuilder Sql {
            get {
                if (this.sql.Length == 0) {
                    WriteSqlElements();
                }

                return this.sql;
            }
        }

        private StringBuilder sql;

        private Queue<ISqlElement> sqlElements;

        public DynamicParameters Parameters { get; private set; }

        private ConstantChecker constantChecker;

        public WhereClauseExpressionVisitor(ISqlDialect dialect, IConfiguration config, FetchNode rootNode) {
            this.sql = new StringBuilder();
            this.sqlElements = new Queue<ISqlElement>();
            this.Parameters = new DynamicParameters();
            this.chainedEntityNames = new Stack<string>();
            this.dialect = dialect;
            this.configuration = config;
            this.rootNode = rootNode;
            this.aliasCounter = 99; // what's the chance of fetching 99 tables??
            this.constantChecker = new ConstantChecker();
        }

        private void WriteSqlElements() {
            while (this.sqlElements.Count > 0) {
                this.sqlElements.Dequeue().Append(this.sql, this.dialect);
            }
        }

        internal FetchNode VisitWhereClause(Expression whereClause) {
            this.sql.Clear();
            this.sqlElements.Clear();
            this.VisitTree(whereClause);
            return this.rootNode;
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            this.isInBinaryComparisonExpression = b.NodeType != ExpressionType.AndAlso && b.NodeType != ExpressionType.OrElse;

            // if this bianry expression does not contain any parameters it's constant so just invoke the thing
            this.constantChecker.Reset();
            this.constantChecker.VisitTree(b);
            if (!this.constantChecker.HasParams) {
                var value = Expression.Lambda(b).Compile().DynamicInvoke(null);
                this.AddParameter(value);
            } 
            else {
                // left hand side of expression
                this.sqlElements.Enqueue(new StringElement("("));
                this.isTopOfBinary = true;
                this.Visit(b.Left);

                // equality operator
                // TODO What about == entity where entity is null??
                this.sqlElements.Enqueue(new StringElement(this.GetOperator(b.NodeType, b.Right.ToString() == "null")));

                // right hand side of expression
                this.isTopOfBinary = true;
                this.Visit(b.Right);
                this.sqlElements.Enqueue(new StringElement(")"));
            }

            this.isInBinaryComparisonExpression = false;
            return b;
        }

        protected override Expression VisitParameter(ParameterExpression p) {
            // if this is the first thing on the lhs or rhs of a binary then we should chuck the primary key in the clause
            if (this.isTopOfBinary) {
                this.sqlElements.Enqueue(new ColumnElement(this.rootNode, this.configuration.GetMap(p.Type).PrimaryKey.DbName, true));
            }

            return p;
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {
            if (m.Expression == null) { // Static property e.g. DateTime.Now
                this.AddParameter(Expression.Lambda(m).Compile().DynamicInvoke(null));
                return m;
            }

            this.isTopOfBinary = false;
            switch (m.Expression.NodeType) {
                case ExpressionType.MemberAccess:
                    if (this.isChainedMemberAccess) {
                        if (this.getForeignKeyName) {
                            // at this point let's fetch the foreign key name
                            this.chainedColumnName = this.configuration.GetMap(m.Member.DeclaringType).Columns[m.Member.Name].DbName;
                            this.chainedColumnType = m.Member.DeclaringType;
                            this.getForeignKeyName = false;
                        }
                        else {
                            this.chainedEntityNames.Push(m.Member.Name);
                        }
                    } else {
                        this.isChainedMemberAccess = true;
                        this.chainedMemberAccessExpression = m;
                        var propInfo = m.Member as PropertyInfo;

                        if (propInfo != null && !propInfo.PropertyType.IsValueType && this.configuration.HasMap(propInfo.PropertyType)) {
                            // here we're doing a e.Entity == entity so get primary key underlying
                            this.chainedColumnName = this.configuration.GetMap(m.Member.DeclaringType).Columns[m.Member.Name].DbName;
                            this.chainedColumnType = m.Member.ReflectedType;
                            this.chainedEntityNames.Push(m.Member.Name);
                        } else if (this.configuration.HasMap(m.Member.DeclaringType)) {
                            // we want to check for a primary key here because in that case we can put the where clause on the referencing object
                            if (this.configuration.GetMap(m.Member.DeclaringType).PrimaryKey.Name == m.Member.Name) {
                                this.getForeignKeyName = true;
                            } else {
                                // we need this column name
                                this.chainedColumnName = this.configuration.GetMap(m.Member.DeclaringType).Columns[m.Member.Name].DbName;
                                this.chainedColumnType = m.Member.DeclaringType;
                            }
                        }
                    }

                    break;

                case ExpressionType.Constant:
                    this.isClosureConstantAccess = true;
                    this.constantMemberAccessName = m.Member.Name;
                    break;

                case ExpressionType.Convert:
                    this.VisitMemberExpressionParameter(m, ((UnaryExpression)m.Expression).Operand.Type);
                    break;

                case ExpressionType.Parameter:
                    this.VisitMemberExpressionParameter(m, m.Expression.Type);
                    break;
            }

            var expr = base.VisitMemberAccess(m);
            this.isChainedMemberAccess = false;
            this.isClosureConstantAccess = false;

            if (!isInBinaryComparisonExpression && m.Type == typeof(Boolean)) {
                // add == 1 to the thing
                this.sqlElements.Enqueue(new StringElement(" = 1"));
            }

            return expr;
        }

        private void VisitMemberExpressionParameter(MemberExpression m, Type declaringType) {
            if (this.isChainedMemberAccess) {
                if (this.getForeignKeyName) {
                    // we're at the bottom and we need to reference the foreign key column name
                    this.sqlElements.Enqueue(new ColumnElement(this.rootNode, this.configuration.GetMap(declaringType).Columns[m.Member.Name].DbName, true));
                    this.getForeignKeyName = false;
                }
                else {
                    // we need to find the alias
                    // we've got chained entity names and the root node
                    if (this.rootNode == null) {
                        this.rootNode = new FetchNode {
                            Alias = "t"
                        };

                        // update exising ISqlElements to use new rootnode (and alias)
                        foreach (var element in this.sqlElements) {
                            var columnElement = element as ColumnElement;
                            if (columnElement != null && columnElement.IsRoot) {
                                columnElement.Node = this.rootNode;
                            }
                        }
                    }

                    this.chainedEntityNames.Push(m.Member.Name);
                    var currentNode = this.rootNode;
                    for (int i = 0, c = this.chainedEntityNames.Count; i < c; ++i) {
                        var propName = this.chainedEntityNames.Pop();
                        if (!currentNode.Children.ContainsKey(propName)) {
                            // create the new node with isFetched = false
                            var newNode = new FetchNode {
                                Alias = "t_" + ++this.aliasCounter,
                                IsFetched = false,
                                Parent = currentNode,
                                Column = this.configuration.GetMap(declaringType).Columns[propName]
                            };

                            if (currentNode.Children.Any()) {
                                var j = 0;
                                var inserted = false;
                                foreach (var child in currentNode.Children) {
                                    if (child.Value.Column.FetchId > newNode.Column.FetchId) {
                                        currentNode.Children.Insert(j, new KeyValuePair<string, FetchNode>(propName, newNode));
                                        inserted = true;
                                        break;
                                    }

                                    j++;
                                }

                                if (!inserted) {
                                    currentNode.Children.Add(propName, newNode);
                                }
                            }
                            else {
                                currentNode.Children.Add(propName, newNode);
                            }
                        }

                        currentNode = currentNode.Children[propName];
                        declaringType = currentNode.Column.Type;
                    }

                    if (this.insideSubQuery) {
                        this.sqlElements.Enqueue(new ColumnElement(null, this.chainedColumnName, false));
                    }
                    else {
                        this.sqlElements.Enqueue(new ColumnElement(currentNode, this.chainedColumnName, false));
                    }
                }
            }
            else {
                this.sqlElements.Enqueue(
                    new ColumnElement(this.rootNode, this.configuration.GetMap(declaringType).Columns[m.Member.Name].DbName, true));
            }
        }

        protected override Expression VisitUnary(UnaryExpression u) {
            if (u.NodeType == ExpressionType.Not) {
                this.sqlElements.Enqueue(new StringElement("not ("));
            }

            base.VisitUnary(u);

            if (u.NodeType == ExpressionType.Not) {
                this.sqlElements.Enqueue(new StringElement(")"));
            }

            return u;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            Expression memberExpr = null;
            Expression valuesExpr = null;

            switch (m.Method.Name) {
                case "Contains":
                    if (m.Method.DeclaringType == typeof(string)) {
                        // this is string.Contains method
                        memberExpr = m.Object;
                        valuesExpr = m.Arguments[0];
                        this.Visit(memberExpr);
                        this.sqlElements.Enqueue(new StringElement(" like "));
                        this.doPrependValue = this.doAppendValue = true;
                        this.prependValue = this.appendValue = "%";
                        this.Visit(valuesExpr);
                        this.doPrependValue = this.doAppendValue = false;
                        return m;
                    }

                    // this is IEnumerable.Contains method
                    if (m.Method.DeclaringType == typeof(Enumerable)) {
                        // static method
                        memberExpr = m.Arguments[1] as MemberExpression;
                        valuesExpr = m.Arguments[0];
                    } else {
                        // contains on IList
                        memberExpr = m.Arguments[0] as MemberExpression;
                        valuesExpr = m.Object;
                    }

                    this.Visit(memberExpr);
                        this.sqlElements.Enqueue(new StringElement(" in "));
                    this.Visit(valuesExpr);
                    break;

                case "StartsWith":
                    memberExpr = m.Object;
                    valuesExpr = m.Arguments[0];
                    this.Visit(memberExpr);
                        this.sqlElements.Enqueue(new StringElement(" like "));
                    this.doAppendValue = true;
                    this.appendValue = "%";
                    this.Visit(valuesExpr);
                    this.doAppendValue = false;
                    break;

                case "EndsWith":
                    memberExpr = m.Object;
                    valuesExpr = m.Arguments[0];
                    this.Visit(memberExpr);
                        this.sqlElements.Enqueue(new StringElement(" like "));
                    this.doPrependValue = true;
                    this.prependValue = "%";
                    this.Visit(valuesExpr);
                    this.doPrependValue = false;
                    break;

                case "Any":
                    memberExpr = m.Arguments[1];
                    var relatedType = m.Arguments[0].Type.GenericTypeArguments[0];
                    var map = this.configuration.GetMap(relatedType);
                    this.sqlElements.Enqueue(new StringElement("exists (select 1 from "));
                    this.dialect.AppendQuotedTableName(this.Sql, map);
                    this.sqlElements.Enqueue(new StringElement(" where "));
                    this.insideSubQuery = true;
                    this.Visit(m.Arguments[1]);
                    this.sqlElements.Enqueue(new StringElement(")"));
                    this.insideSubQuery = false;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return m;
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            object value = null;

            if (this.isClosureConstantAccess) {
                this.getForeignKeyName = false; // tidy up this property
                if (this.isChainedMemberAccess) {
                    value = Expression.Lambda(this.chainedMemberAccessExpression).Compile().DynamicInvoke(null);
                }
                else {
                    var currentType = c.Value.GetType();
                    while (currentType != null) {
                        var field = currentType.GetField(this.constantMemberAccessName, BindingFlags.Public | BindingFlags.Instance);

                        // if the field exists, get its value and stop
                        if (field != null) {
                            value = field.GetValue(c.Value);
                            break;
                        }

                        // else look in the base type
                        if (currentType.BaseType == null) {
                            throw new Exception("Couldn't find a value while visiting a constant expression");
                        }

                        currentType = currentType.BaseType;
                    }
                }
            }
            else {
                value = c.Value;
            }

            if (value != null) {
                if (this.configuration.HasMap(value.GetType())) {
                    // fetch the primary key
                    value = this.configuration.GetMap(value.GetType()).GetPrimaryKeyValue(value);
                }

                this.AddParameter(value);
            }

            return c;
        }

        private void AddParameter(object value) {
            this.sqlElements.Enqueue(new StringElement("@l_" + ++this.paramCounter));
            this.Parameters.Add("@l_" + this.paramCounter, this.doAppendValue || this.doPrependValue ? this.WrapValue(value) : value);
        }

        private object WrapValue(object value) {
            if (this.doPrependValue) {
                value = this.prependValue + value;
            }

            if (this.doAppendValue) {
                value = value + this.appendValue;
            }

            return value;
        }

        private string GetOperator(ExpressionType nodeType, bool isNull) {
            switch (nodeType) {
                case ExpressionType.Equal:
                    return isNull ? " is " : " = ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.NotEqual:
                    return isNull ? " is not " : " != ";
                case ExpressionType.AndAlso:
                    return " and ";
                case ExpressionType.OrElse:
                    return " or ";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}