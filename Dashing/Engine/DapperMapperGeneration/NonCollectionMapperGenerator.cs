namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if COREFX
    using System.Reflection;
#endif

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DML;

    internal class NonCollectionMapperGenerator : INonCollectionMapperGenerator {
        private readonly IConfiguration configuration;

        public NonCollectionMapperGenerator(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public Tuple<Delegate, Type[]> GenerateNonCollectionMapper<T>(FetchNode fetchTree) {
            // this is simple, just take the arguments, map them
            // params go in order of fetch tree
            var tt = typeof(T);
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(tt);
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { tt };

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, tt);

            // go through the tree
            int i = 1;
            statements.AddRange(this.VisitNonCollectionTree<T>(fetchTree, objectsParam, rootVar, ref i, mappedTypes));

            // add in the return statement and parameter
            statements.Add(Expression.Call(rootVar, tt.GetMethod("EnableTracking")));
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Block(new[] { rootVar }, statements), objectsParam).Compile(), mappedTypes.ToArray());
        }

        private static void GetRootAssignment<T>(
            List<Expression> statements,
            ParameterExpression rootVar,
            ParameterExpression objectsParam,
            Type rootType) {
            statements.Add(Expression.Assign(rootVar, Expression.Convert(Expression.ArrayIndex(objectsParam, Expression.Constant(0)), rootType)));
        }

        private IEnumerable<Expression> VisitNonCollectionTree<T>(
            FetchNode fetchTree,
            ParameterExpression objectsParam,
            Expression parent,
            ref int i,
            IList<Type> mappedTypes) {
            var statements = new List<Expression>();
            foreach (var child in fetchTree.Children) {
                if (child.Value.IsFetched) {
                    var propExpr = Expression.Property(parent, child.Value.Column.Name);
                    var indexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                    var ifExpr = Expression.NotEqual(indexExpr, Expression.Constant(null));
                    var mappedType = child.Value.Column.Type;
                    var assignExpr = Expression.Assign(propExpr, Expression.Convert(indexExpr, mappedType));
                    ++i;
                    mappedTypes.Add(mappedType);
                    var innerStatements = this.VisitNonCollectionTree<T>(child.Value, objectsParam, propExpr, ref i, mappedTypes);

                    var thenExpr = new List<Expression> { assignExpr };
                    thenExpr.AddRange(innerStatements);
                    thenExpr.Add(
                        Expression.Call(Expression.Convert(indexExpr, typeof(ITrackedEntity)), typeof(ITrackedEntity).GetMethod("EnableTracking")));
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
            }

            return statements;
        }
    }
}