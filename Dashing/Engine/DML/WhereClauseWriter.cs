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

    internal sealed class WhereClauseWriter : IWhereClauseWriter {
        private readonly ISqlDialect dialect;

        private readonly IConfiguration config;

        private FetchNode modifiedRootNode;

        private FetchNode currentNode;

        private List<PropertyInfo> currentFetchStack;

        private IList<IList<PropertyInfo>> currentFetchStacks;

        private Queue<ISqlElement> sqlElements;

        private AutoNamingDynamicParameters autoNamingDynamicParameters;

        private bool isConstantExpression;

        private bool isTopOfBinaryOrMethod;

        private bool doAppendValue;

        private bool doPrependValue;

        private string prependValue;

        private string appendValue;

        private int aliasCounter;

        private bool isPrimaryKeyAccess;

        private bool isEntityFetch;

        private Type entityFetchType;

        private Type primaryKeyAccessType;

        private bool isNegated;

        private bool isInBinaryComparisonExpression;

        private object value;

        public WhereClauseWriter(ISqlDialect dialect, IConfiguration config) {
            this.dialect = dialect;
            this.config = config;
        }

        public SelectWriterResult GenerateSql<T>(IEnumerable<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode, AutoNamingDynamicParameters parameters) {
            if (whereClauses.IsEmpty()) {
                return new SelectWriterResult(string.Empty, null, rootNode);
            }

            this.InitVariables();
            this.autoNamingDynamicParameters = parameters;
            this.modifiedRootNode = rootNode;

            foreach (var whereClause in whereClauses) {
                this.VisitWhereClause(whereClause);
                this.sqlElements.Enqueue(new StringElement(" and "));
            }

            this.InferInnerJoins();
            return new SelectWriterResult(this.GetSql(), this.autoNamingDynamicParameters, this.modifiedRootNode);
        }

        private void InferInnerJoins() {
            if (this.modifiedRootNode == null) {
                return;
            }

            foreach (var fetchStack in this.currentFetchStacks) {
                var currentNodeDownStack = this.modifiedRootNode;
                foreach (var propertyInfo in fetchStack) {
                    currentNodeDownStack = currentNodeDownStack.Children[propertyInfo.Name];
                    currentNodeDownStack.InferredInnerJoin = true;
                }
            }
        }

        private void InitVariables() {
            this.sqlElements = new Queue<ISqlElement>();
            this.sqlElements.Enqueue(new StringElement(" where "));
            this.aliasCounter = 99;
            this.currentFetchStacks = new List<IList<PropertyInfo>>();
        }

        private void VisitWhereClause<T>(Expression<Func<T, bool>> whereClause) {
            this.ResetVariables();
            var el = this.Visit(whereClause.Body);
            if (el != null) {
                this.sqlElements.Enqueue(el);
                this.sqlElements.Enqueue(new StringElement(this.isNegated ? " = 0" : " = 1"));
                if (this.currentFetchStack.Any()) {
                    this.currentFetchStacks.Add(this.currentFetchStack);
                }
            }
        }

        private void ResetVariables() {
            this.isTopOfBinaryOrMethod = true;
            this.isConstantExpression = false;
            this.doAppendValue = false;
            this.doPrependValue = false;
            this.isPrimaryKeyAccess = false;
            this.isNegated = false;
            this.isEntityFetch = false;
            this.currentFetchStack = new List<PropertyInfo>();
        }

        private ISqlElement VisitMethodCall(MethodCallExpression exp) {
            Expression memberExpr = null;
            Expression valuesExpr = null;
            var isCurrentlyNegated = this.isNegated;

            switch (exp.Method.Name) {
                case "Equals":
                    this.VisitBinary(Expression.MakeBinary(ExpressionType.Equal, exp.Object, exp.Arguments[0]));
                    break;

                case "Contains":
                    if (exp.Method.DeclaringType == typeof(string)) {
                        // this is string.Contains method
                        memberExpr = exp.Object;
                        valuesExpr = exp.Arguments[0];
                        this.ResetVariables();
                        var memberEl = this.Visit(memberExpr);
                        if (!this.isConstantExpression && memberEl != null) {
                            this.ResetVariables();
                            this.doPrependValue = this.doAppendValue = true;
                            this.prependValue = this.appendValue = "%";
                            var valuesEl = this.Visit(valuesExpr);
                            if (this.isConstantExpression) {
                                if (valuesEl == null) {
                                    valuesEl = this.AddParameter(this.GetDynamicValue(valuesExpr));
                                }

                                this.sqlElements.Enqueue(memberEl);
                                if (isCurrentlyNegated) {
                                    this.sqlElements.Enqueue(new StringElement(" not like "));
                                }
                                else {
                                    this.sqlElements.Enqueue(new StringElement(" like "));
                                }

                                this.sqlElements.Enqueue(valuesEl);
                                this.ResetVariables();
                                return null;
                            }
                        }
                    }
                    else {
                        // this is IEnumerable.Contains method
                        if (exp.Method.DeclaringType == typeof(Enumerable)) {
                            // static method
                            if (exp.Arguments[1].NodeType == ExpressionType.Parameter) {
                                memberExpr = exp.Arguments[1] as ParameterExpression;
                            }
                            else {
                                memberExpr = exp.Arguments[1] as MemberExpression;
                            }

                            valuesExpr = exp.Arguments[0];
                        }
                        else {
                            // contains on IList
                            if (exp.Arguments[0].NodeType == ExpressionType.Parameter) {
                                memberExpr = exp.Arguments[0] as ParameterExpression;
                            }
                            else {
                                memberExpr = exp.Arguments[0] as MemberExpression;
                            }

                            valuesExpr = exp.Object;
                        }

                        this.ResetVariables();
                        var containsMemberEl = this.Visit(memberExpr);
                        if (!this.isConstantExpression && containsMemberEl != null) {
                            var isEntityFetch = this.isEntityFetch;
                            this.ResetVariables();
                            var valuesEl = this.Visit(valuesExpr);
                            if (this.isConstantExpression) {
                                if (valuesEl == null) {
                                    var dynamicValue = this.GetDynamicValue(valuesExpr);
                                    if (isEntityFetch) {
                                        // we're got a IEnumerable<Entity> here so need to change to IEnumerable<ValueType>
                                        var entityMap = this.config.GetMap(entityFetchType);
                                        var primaryKeyType = entityMap.PrimaryKey.Type;
                                        var param = Expression.Parameter(entityFetchType);
                                        var selectExpr =
                                            Expression.Lambda(
                                                Expression.MakeMemberAccess(param, entityFetchType.GetProperty(entityMap.PrimaryKey.Name)),
                                                param).Compile();
                                        var projectionMethod =
                                            typeof(Enumerable).GetMethods()
                                                              .First(
                                                                  m =>
                                                                  m.Name == "Select"
                                                                  && m.GetParameters().Any(p => p.ParameterType.GetGenericArguments().Length == 2))
                                                              .MakeGenericMethod(entityFetchType, primaryKeyType);
                                        //var projectionMethod = typeof(Enumerable).GetMethod("Select", new Type[] { typeof(IEnumerable<>).MakeGenericType(entityFetchType), typeof(Func<,>).MakeGenericType(entityFetchType, primaryKeyType) });
                                        dynamicValue = projectionMethod.Invoke(null, new[] { dynamicValue, selectExpr });
                                    }

                                    valuesEl = this.AddParameter(dynamicValue);
                                }

                                this.sqlElements.Enqueue(containsMemberEl);
                                if (isCurrentlyNegated) {
                                    this.sqlElements.Enqueue(new StringElement(" not in "));
                                }
                                else {
                                    this.sqlElements.Enqueue(new StringElement(" in "));
                                }

                                this.sqlElements.Enqueue(valuesEl);
                                this.ResetVariables();
                                return null;
                            }
                        }
                    }

                    break;

                case "StartsWith":
                    memberExpr = exp.Object;
                    valuesExpr = exp.Arguments[0];
                    this.ResetVariables();
                    var startsWithMemberEl = this.Visit(memberExpr);
                    if (!this.isConstantExpression && startsWithMemberEl != null) {
                        this.ResetVariables();
                        this.doAppendValue = true;
                        this.appendValue = "%";
                        var valuesEl = this.Visit(valuesExpr);
                        if (this.isConstantExpression) {
                            if (valuesEl == null) {
                                valuesEl = this.AddParameter(this.GetDynamicValue(valuesExpr));
                            }

                            this.sqlElements.Enqueue(startsWithMemberEl);
                            if (isCurrentlyNegated) {
                                this.sqlElements.Enqueue(new StringElement(" not like "));
                            }
                            else {
                                this.sqlElements.Enqueue(new StringElement(" like "));
                            }

                            this.sqlElements.Enqueue(valuesEl);
                            this.ResetVariables();
                            return null;
                        }
                    }

                    break;

                case "EndsWith":
                    memberExpr = exp.Object;
                    valuesExpr = exp.Arguments[0];
                    this.ResetVariables();
                    var endsWithMemberEl = this.Visit(memberExpr);
                    if (!this.isConstantExpression && endsWithMemberEl != null) {
                        this.ResetVariables();
                        this.doPrependValue = true;
                        this.prependValue = "%";
                        var valuesEl = this.Visit(valuesExpr);
                        if (this.isConstantExpression) {
                            if (valuesEl == null) {
                                valuesEl = this.AddParameter(this.GetDynamicValue(valuesExpr));
                            }

                            this.sqlElements.Enqueue(endsWithMemberEl);
                            if (isCurrentlyNegated) {
                                this.sqlElements.Enqueue(new StringElement(" not like "));
                            }
                            else {
                                this.sqlElements.Enqueue(new StringElement(" like "));
                            }

                            this.sqlElements.Enqueue(valuesEl);
                            return null;
                        }
                    }

                    break;

                case "Any":
                    memberExpr = exp.Arguments[1]; // c => c.Content == value()
                    var columnWithAnyExpression = exp.Arguments[0] as MemberExpression; // p.Comments
                    if (columnWithAnyExpression != null) {
                        var columnType = columnWithAnyExpression.Type.GenericTypeArguments.First();

                        // we use a new whereclausewriter to generate the inner statement
                        var innerSelectWriter = new SelectWriter(this.dialect, this.config);
                        var selectQuery = Activator.CreateInstance(
                            typeof(SelectQuery<>).MakeGenericType(columnType),
                            new NonExecutingSelectQueryExecutor());
                        var whereMethod = selectQuery.GetType().GetMethod(nameof(SelectQuery<object>.Where));
                        whereMethod.Invoke(selectQuery, new object[] { memberExpr });
                        var generateSqlMethod = innerSelectWriter.GetType().GetMethod( nameof(SelectWriter.GenerateSql)).MakeGenericMethod(columnType);
                        var innerStatement = (SelectWriterResult)generateSqlMethod.Invoke(innerSelectWriter, new[] { selectQuery, this.autoNamingDynamicParameters, true });

                        // remove the columns from the expression
                        // TODO tell the select writer to not do this in the first place
                        var fromIdx = innerStatement.Sql.IndexOf(" from ");
                        innerStatement.Sql = "select 1" + innerStatement.Sql.Substring(fromIdx);

                        // re-write the aliases to not use t_ as that's probably in the outer query
                        // TODO speed this up
                        // TODO Support nested anys!
                        innerStatement.Sql = innerStatement.Sql.Replace(" t ", " i ").Replace("t_", "i_").Replace("t.", "i.");
                        //innerStatement.Sql = innerStatement.Sql.Replace("t_", "i_");

                        // add the column sql
                        this.EnsureRootNodeExists(); // we need everything to have an alias now
                        this.ResetVariables();
                        var columnElement = (ColumnElement)this.Visit(columnWithAnyExpression); // this ensures we're joining the correct stuff
                        if (isCurrentlyNegated) {
                            this.sqlElements.Enqueue(new StringElement("not exists ("));
                        }
                        else {
                            this.sqlElements.Enqueue(new StringElement("exists ("));
                        }

                        // add the reference back to the related column
                        var thisTinyBitOfSql = new StringBuilder(" and ");
                        var mapPrimaryKey = this.config.GetMap(columnWithAnyExpression.Expression.Type).PrimaryKey.DbName;
                        thisTinyBitOfSql.Append(columnElement.Node != null ? columnElement.Node.Alias : "t");
                        thisTinyBitOfSql.Append(".");
                        this.dialect.AppendQuotedName(thisTinyBitOfSql, mapPrimaryKey);
                        thisTinyBitOfSql.Append(" = ");
                        thisTinyBitOfSql.Append("i.");
                        this.dialect.AppendQuotedName(
                            thisTinyBitOfSql,
                            this.config.GetMap(columnWithAnyExpression.Expression.Type).Columns[columnWithAnyExpression.Member.Name].ChildColumn
                                                                                                                                    .DbName);
                        
                        // finish off the sql
                        this.sqlElements.Enqueue(new StringElement(innerStatement.Sql + thisTinyBitOfSql));
                        this.sqlElements.Enqueue(new StringElement(")"));

                        return null;
                    }

                    break;
            }

            this.isConstantExpression = true;
            return null;
        }

        private ISqlElement VisitMemberAccess(MemberExpression exp) {
            if (exp.Expression == null) {
                // static property
                if (this.isTopOfBinaryOrMethod) {
                    // quicker path
                    var propInfo = exp.Member as PropertyInfo;
                    if (propInfo != null) {
                        return this.AddParameter(propInfo.GetValue(null));
                    }
                    var fieldInfo = exp.Member as FieldInfo;
                    if (fieldInfo != null) {
                        return this.AddParameter(fieldInfo.GetValue(null));
                    }

                    // slow path
                    return this.AddParameter(this.GetDynamicValue(exp));
                }

                return null;
            }

            var declaringType = exp.Expression.NodeType == ExpressionType.Convert
                                    ? ((UnaryExpression)exp.Expression).Operand.Type
                                    : exp.Expression.Type;

            // redirect for .HasValue expressions on nullables
            if (declaringType.IsGenericType() && declaringType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                var propInfo = exp.Member as PropertyInfo;
                if (propInfo != null && propInfo.Name == "HasValue") {
                    // re-write to binary expression
                    return this.Visit(Expression.MakeBinary(this.isNegated ? ExpressionType.Equal : ExpressionType.NotEqual, exp.Expression, Expression.Constant(null)));
                }
            }

            var isTopOfBinaryOrMethodCopy = this.isTopOfBinaryOrMethod;
            if (isTopOfBinaryOrMethodCopy && this.config.HasMap(declaringType) && this.config.GetMap(declaringType).PrimaryKey.Name == exp.Member.Name) {
                this.isPrimaryKeyAccess = true;
                this.primaryKeyAccessType = declaringType;
            }

            this.isTopOfBinaryOrMethod = false;
            var next = this.Visit(exp.Expression);
            if (!this.isConstantExpression) {
                var propInfo = exp.Member as PropertyInfo;
                if (propInfo == null) {
                    throw new NotImplementedException();
                }

                if (!isTopOfBinaryOrMethodCopy && this.config.HasMap(propInfo.PropertyType)
                    && (!isPrimaryKeyAccess || primaryKeyAccessType != propInfo.PropertyType)) {
                    // check that the fetch tree is correct
                    this.EnsureRootNodeExists();
                    this.GetOrCreateCurrentNode(propInfo, declaringType);
                }
            }
            else {
                if (value != null) {
                    var propInfo = exp.Member as PropertyInfo;
                    if (propInfo != null) {
                        value = propInfo.GetValue(value);
                    }
                    else {
                        var fieldInfo = exp.Member as FieldInfo;
                        if (fieldInfo != null) {
                            value = fieldInfo.GetValue(value);
                        }
                        else {
                            value = null;
                        }
                    }
                }
            }

            if (isTopOfBinaryOrMethodCopy) {
                if (this.isConstantExpression) {
                    if (this.isInBinaryComparisonExpression && value != null) {
                        return this.AddParameter(value);
                    }

                    return null;
                }

                // ok, let's figure out which column and table reference we need
                var propInfo = exp.Member as PropertyInfo;
                if (propInfo == null) {
                    throw new NotImplementedException();
                }

                if (this.config.HasMap(propInfo.PropertyType)) {
                    // we're doing e.Entity == entity (or similar)
                    var fkName = this.config.GetMap(declaringType).Columns[propInfo.Name].DbName;
                    isEntityFetch = true;
                    entityFetchType = propInfo.PropertyType;
                    return new ColumnElement(this.currentNode, fkName, exp.Expression.NodeType != ExpressionType.MemberAccess);
                }

                if (this.config.HasMap(declaringType)) {
                    if (this.config.GetMap(declaringType).PrimaryKey.Name == propInfo.Name) {
                        if (exp.Expression.NodeType == ExpressionType.MemberAccess) {
                            // e.Parent.ParentId == blah => get ParentId on e instead
                            var foreignKeyExpression = ((MemberExpression)exp.Expression);
                            var foreignKeyName =
                                this.config.GetMap(foreignKeyExpression.Expression.Type).Columns[foreignKeyExpression.Member.Name].DbName;
                            return new ColumnElement(
                                this.currentNode,
                                foreignKeyName,
                                foreignKeyExpression.Expression.NodeType != ExpressionType.MemberAccess);
                        }
                        // e.EntityId == blah
                        return new ColumnElement(this.modifiedRootNode, this.config.GetMap(declaringType).Columns[propInfo.Name].DbName, true);
                    }
                    return new ColumnElement(
                        this.currentNode,
                        this.config.GetMap(declaringType).Columns[propInfo.Name].DbName,
                        exp.Expression.NodeType != ExpressionType.MemberAccess);
                }
            }

            return null;
        }

        private void GetOrCreateCurrentNode(PropertyInfo propInfo, Type declaringType) {
            this.currentFetchStack.Add(propInfo);
            if (!this.currentNode.Children.ContainsKey(propInfo.Name)) {
                // create the node
                var newNode = new FetchNode {
                                                Alias = "t_" + ++this.aliasCounter,
                                                IsFetched = false,
                                                Parent = this.currentNode,
                                                Column = this.config.GetMap(declaringType).Columns[propInfo.Name]
                                            };
                if (this.currentNode.Children.Any()) {
                    var i = 0;
                    var inserted = false;
                    foreach (var child in this.currentNode.Children) {
                        if (child.Value.Column.FetchId > newNode.Column.FetchId) {
                            this.currentNode.Children.Insert(i, new KeyValuePair<string, FetchNode>(propInfo.Name, newNode));
                            inserted = true;
                            break;
                        }

                        ++i;
                    }

                    if (!inserted) {
                        this.currentNode.Children.Add(propInfo.Name, newNode);
                    }
                }
                else {
                    this.currentNode.Children.Add(propInfo.Name, newNode);
                }

                this.currentNode = newNode;
            }
            else {
                this.currentNode = this.currentNode.Children[propInfo.Name];
            }
        }

        private void EnsureRootNodeExists() {
            if (this.modifiedRootNode == null) {
                this.modifiedRootNode = new FetchNode { Alias = "t" };

                // update exising ISqlElements to use new rootnode (and alias)
                foreach (var element in this.sqlElements) {
                    var columnElement = element as ColumnElement;
                    if (columnElement != null && columnElement.IsRoot) {
                        columnElement.Node = this.modifiedRootNode;
                    }
                }

                this.currentNode = this.modifiedRootNode;
            }
        }

        private ISqlElement VisitParameter(ParameterExpression exp) {
            if (this.isTopOfBinaryOrMethod) {
                this.isEntityFetch = true;
                this.entityFetchType = exp.Type;
                return new ColumnElement(this.modifiedRootNode, this.config.GetMap(exp.Type).PrimaryKey.DbName, true);
            }

            this.currentNode = this.modifiedRootNode;
            return null;
        }

        private ISqlElement VisitConstant(ConstantExpression exp) {
            this.isConstantExpression = true;

            if (exp.Value != null && this.isTopOfBinaryOrMethod && (exp.Type.GetEnumerableType() == null || !this.config.HasMap(exp.Type.GetEnumerableType()))) {
                return AddParameter(exp.Value);
            }

            value = exp.Value;
            return null;
        }

        private object GetDynamicValue(Expression expr) {
            return Expression.Lambda(expr).Compile().DynamicInvoke(null);
        }

        private ConstantElement AddParameter(object value) {
            if (value != null) {
                var valueType = value.GetType();
                if (!valueType.IsValueType() && this.config.HasMap(valueType)) {
                    value = this.config.GetMap(valueType).GetPrimaryKeyValue(value);
                }
            }

            var name = this.autoNamingDynamicParameters.Add(this.doAppendValue || this.doPrependValue ? this.WrapValue(value) : value);
            return new ConstantElement(name, value);
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

        private ISqlElement VisitUnary(UnaryExpression exp) {
            if (exp.NodeType == ExpressionType.Not) {
                this.isNegated = true;
                if (exp.Operand is BinaryExpression) {
                    this.sqlElements.Enqueue(new StringElement("not "));
                }
            }

            return this.Visit(exp.Operand);
        }

        private ISqlElement VisitBinary(BinaryExpression exp) {
            this.isInBinaryComparisonExpression = this.IsInBinaryComparisonExpression(exp.NodeType);
            var isInAndOrOrExpression = exp.NodeType == ExpressionType.AndAlso || exp.NodeType == ExpressionType.OrElse;

            if (this.isInBinaryComparisonExpression) {
                this.VisitBinaryComparison(exp);
            }
            else if (isInAndOrOrExpression) {
                this.VisitAndOrBinary(exp);
            }
            else {
                // we're almost certainly inside some constant expression here so we'll let it go
                this.isConstantExpression = true;
            }

            return null;
        }

        private void VisitAndOrBinary(BinaryExpression exp) { // we're got an and or an or so we just visit the sides as at some point they'll hit the code above
            this.sqlElements.Enqueue(new StringElement("("));
            this.ResetVariables();
            var left = this.Visit(exp.Left);
            if (left != null) {
                this.sqlElements.Enqueue(left); // for boolean type stuff
                this.sqlElements.Enqueue(new StringElement(this.isNegated ? " = 0" : " = 1"));
            }

            var leftFetchStacks = this.currentFetchStacks;
            this.currentFetchStacks = new List<IList<PropertyInfo>>();

            this.sqlElements.Enqueue(new StringElement(this.GetOperator(exp.NodeType, false)));
            this.ResetVariables();
            var right = this.Visit(exp.Right);
            if (right != null) {
                this.sqlElements.Enqueue(right);
                this.sqlElements.Enqueue(new StringElement(this.isNegated ? " = 0" : " = 1"));
            }

            var rightFetchStacks = this.currentFetchStacks;
            if (exp.NodeType == ExpressionType.AndAlso) {
                // we union the left and right hand side fetch stacks
                this.currentFetchStacks = leftFetchStacks.Union(rightFetchStacks).ToList();
            } else if (exp.NodeType == ExpressionType.OrElse) {
                // we intersect the lists taking the common denominator fetch paths
                // i.e. p.X.Y intersecting p.X.Z leaves p.X
                var result = new List<IList<PropertyInfo>>();
                foreach (var stack in leftFetchStacks) {
                    foreach (var comparisonStack in rightFetchStacks) {
                        for (var i = 0; i < stack.Count; i++) {
                            var stackPropInfo = stack[i];
                            if (i >= comparisonStack.Count) {
                                // reached the end of comparison
                                if (i > 0) {
                                    result.Add(stack.Take(i).ToList());
                                }
                            } else {
                                var comparisonPropInfo = comparisonStack[i];
                                if (comparisonPropInfo.MetadataToken != stackPropInfo.MetadataToken
                                    || !Equals(comparisonPropInfo.Module, stackPropInfo.Module)) {
                                    // not the same prop info, take what we've got so far
                                    if (i > 1) {
                                        result.Add(stack.Take(i - 1).ToList());
                                    }
                                    else {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                this.currentFetchStacks = result;
            }

            this.sqlElements.Enqueue(new StringElement(")"));
        }

        private void MaybeAddFetchStackToStacks() {
            if (!this.isConstantExpression && this.currentFetchStack.Count > 0) {
                this.currentFetchStacks.Add(this.currentFetchStack);
            }
        }

        private void VisitBinaryComparison(BinaryExpression exp) { 
            // we're inside a comparion here so we'll get the left and right hand sides 
            // and then add the elemts properly
            this.ResetVariables();
            var left = this.Visit(exp.Left);
            this.MaybeAddFetchStackToStacks();
            var isLeftConstantExpression = this.isConstantExpression;

            this.ResetVariables();
            var right = this.Visit(exp.Right);
            this.MaybeAddFetchStackToStacks();
            var isRightConstantExpression = this.isConstantExpression;

            if (isLeftConstantExpression && left == null) {
                left = this.AddParameter(this.GetDynamicValue(exp.Left));
            }

            if (isRightConstantExpression && right == null) {
                var dynamicValue = this.GetDynamicValue(exp.Right);
                right = (dynamicValue == null)
                            ? new ConstantElement("null", null)
                            : this.AddParameter(dynamicValue);
            }

            this.sqlElements.Enqueue(new StringElement("("));
            if (isLeftConstantExpression && ((ConstantElement)left).Value == null) {
                this.sqlElements.Enqueue(right);
                this.sqlElements.Enqueue(new StringElement(this.GetOperator(exp.NodeType, true)));
                this.sqlElements.Enqueue(left);
            }
            else {
                this.sqlElements.Enqueue(left);
                this.sqlElements.Enqueue(new StringElement(this.GetOperator(exp.NodeType, isRightConstantExpression && ((ConstantElement)right).Value == null)));
                this.sqlElements.Enqueue(right);
            }

            this.sqlElements.Enqueue(new StringElement(")"));
            this.isInBinaryComparisonExpression = false;
        }

        private bool IsInBinaryComparisonExpression(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return true;
                default:
                    return false;
            }
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

        private string GetSql() {
            var sql = new StringBuilder();
            while (this.sqlElements.Count > 1) {
                // this is one as there's a trailing and that we don't want
                this.sqlElements.Dequeue().Append(sql, this.dialect);
            }

            return sql.ToString();
        }

        private ISqlElement Visit(Expression exp) {
            if (exp == null) {
                return null;
            }

            switch (exp.NodeType) {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                //case ExpressionType.TypeIs:
                //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                //case ExpressionType.Conditional:
                //    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                //case ExpressionType.Lambda:
                //    return this.VisitLambda((LambdaExpression)exp);
                //case ExpressionType.New:
                //    return this.VisitNew((NewExpression)exp);
                //case ExpressionType.NewArrayInit:
                //case ExpressionType.NewArrayBounds:
                //    return this.VisitNewArray((NewArrayExpression)exp);
                //case ExpressionType.Invoke:
                //    return this.VisitInvocation((InvocationExpression)exp);
                //case ExpressionType.MemberInit:
                //    return this.VisitMemberInit((MemberInitExpression)exp);
                //case ExpressionType.ListInit:
                //    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
    }
}