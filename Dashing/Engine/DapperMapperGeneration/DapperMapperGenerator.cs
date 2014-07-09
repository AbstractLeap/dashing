namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Extensions;

    internal class DapperMapperGenerator : IDapperMapperGenerator {
        private readonly IGeneratedCodeManager generatedCodeManager;

        public DapperMapperGenerator(IGeneratedCodeManager generatedCodeManager) {
            this.generatedCodeManager = generatedCodeManager;
        }

        /// <summary>
        ///     Generates a Func for the passed in fetchTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="fetchTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        public Delegate GenerateCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            // note that we can only fetch one collection at a time
            // so, if there's more than one in the SelectQuery they should be split out prior to calling this
            bool visitedCollection = false;
            var tt = typeof(T);
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");
            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression> { Expression.Parameter(rootType) };
            this.AddDictionaryFetch<T>(dictionaryParam, fetchTree, statements, parameters, isTracked);
            this.VisitTree(fetchTree, statements, parameters, false);

            // add in the return statement and parameter
            statements.Add(parameters.First());
            return Expression.Lambda(Expression.Lambda(Expression.Block(statements), parameters), dictionaryParam).Compile();
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private void AddDictionaryFetch<T>(
            ParameterExpression dictionaryParam,
            FetchNode fetchTree,
            IList<Expression> statements,
            IList<ParameterExpression> parameters,
            bool isTracked) {
            // primary key get expression
            var primaryKeyExpr = Expression.Convert(Expression.Property(parameters.First(), fetchTree.Children.First().Value.Column.Map.PrimaryKey.Name), typeof(object));

            // check the dictionary for this value
            var tt = typeof(T);
            var expr = Expression.Assign(
                parameters.First(),
                Expression.Call(
                    null,
                    typeof(DictionaryExtensions).GetMethods()
                                                .First(m => m.Name == "GetOrAdd" && m.GetParameters().Count() == 3 && m.GetParameters().Count(p => p.Name == "valueCreator") == 0)
                                                .MakeGenericMethod(
                                                    typeof(object),
                                                    isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt)),
                    new Expression[] { dictionaryParam, primaryKeyExpr, parameters.First() }));
            statements.Add(expr);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private void VisitTree(FetchNode node, IList<Expression> statements, IList<ParameterExpression> parameters, bool visitedCollection) {
            var parentParam = parameters.Last();
            foreach (var child in node.Children) {
                // create a param
                Type childType;
                if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                    if (visitedCollection) {
                        throw new InvalidOperationException("You can only generate a mapper for one collection at a time");
                    }

                    childType = child.Value.Column.Type.GetGenericArguments().First();
                    visitedCollection = true;
                }
                else {
                    childType = child.Value.Column.Type;
                }

                var childParam = Expression.Parameter(this.generatedCodeManager.GetForeignKeyType(childType));

                // add the member assign expression, check for null first
                if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                    var ex = Expression.IfThen(
                        Expression.NotEqual(parentParam, Expression.Constant(null)),
                        Expression.Call(
                            Expression.Property(parentParam, child.Value.Column.Name),
                            typeof(ICollection<>).MakeGenericType(child.Value.Column.Type.GetGenericArguments().First()).GetMethod("Add"),
                            new Expression[] { childParam }));
                    statements.Add(ex);
                }
                else {
                    var ex = Expression.IfThen(
                        Expression.NotEqual(parentParam, Expression.Constant(null)),
                        Expression.Assign(Expression.Property(parentParam, child.Value.Column.Name), childParam));
                    statements.Add(ex);
                }

                // add each child node
                parameters.Add(childParam);
                this.VisitTree(child.Value, statements, parameters, visitedCollection);
            }
        }

        public Delegate GenerateMultiCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            var tt = typeof(T);
            var rootType = isTracked
                               ? this.generatedCodeManager.GetTrackingType(tt)
                               : this.generatedCodeManager.GetForeignKeyType(tt);
            var rootParam = Expression.Parameter(rootType);

            // root dictionary stores the root object
            var rootDictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");

            // other dictionary stores all the other collection objects
            var otherDictionaryParam =
                Expression.Parameter(
                    typeof(IDictionary<,>).MakeGenericType(
                        typeof(string),
                        typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object))));

            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression> { rootParam };
            this.AddDictionaryFetch<T>(rootDictionaryParam, fetchTree, statements, parameters, isTracked);
            int collectionFetchParamCounter = 0;
            this.VisitMultiCollectionTree<T>(fetchTree, statements, parameters, otherDictionaryParam, ref collectionFetchParamCounter);

            // add in the return statement and parameter
            statements.Add(rootParam);
            return Expression.Lambda(Expression.Lambda(Expression.Block(statements), parameters), rootDictionaryParam, otherDictionaryParam).Compile();
        }

        private void VisitMultiCollectionTree<T>(FetchNode node, IList<Expression> statements, IList<ParameterExpression> parameters, ParameterExpression otherDictionaryParam, ref int collectionFetchParamCounter) {
            var parentParam = parameters.Last();
            foreach (var child in node.Children) {
                // create a param
                Type childType = child.Value.Column.Relationship == RelationshipType.OneToMany ? child.Value.Column.Type.GetGenericArguments().First() : child.Value.Column.Type;
                ParameterExpression childParam;
                if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                    // for one to many we need to "hard code" the name so that the DelegateQueryCreator can also use the same name
                    childParam =
                        Expression.Parameter(
                            this.generatedCodeManager.GetForeignKeyType(childType),
                            "fetchParam_" + ++collectionFetchParamCounter);
                }
                else {
                    childParam = Expression.Parameter(this.generatedCodeManager.GetForeignKeyType(childType));
                }

                // add the member assign expression
                if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                    // check dictionary for existing instance
                    var dictAccessExpr = Expression.Property(
                        otherDictionaryParam,
                        "Item",
                        Expression.Constant(childParam.Name));
                    var pkPropertyExpr = Expression.Property(
                        childParam,
                        child.Value.Column.ChildColumn.Map.PrimaryKey.Name);
                    var ex =
                        Expression.IfThen(
                            Expression.And(
                                Expression.NotEqual(parentParam, Expression.Constant(null)),
                                Expression.NotEqual(childParam, Expression.Constant(null))),
                            Expression.IfThenElse(
                                Expression.Call(
                                    dictAccessExpr,
                                    typeof(IDictionary<,>).MakeGenericType(
                                        typeof(object),
                                        typeof(object)).GetMethod("ContainsKey"),
                                    Expression.Convert(pkPropertyExpr, typeof(object))),
                                Expression.Assign(
                                    childParam,
                                    Expression.Convert(Expression.Property(dictAccessExpr, "Item", Expression.Convert(pkPropertyExpr, typeof(object))), childType)),
                                Expression.Block(
                                    Expression.Call(
                                        dictAccessExpr,
                                        typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object)).GetMethods()
                                                            .First(
                                                                m => m.Name == "Add" && m.GetParameters().Count() == 2),
                                        Expression.Convert(pkPropertyExpr, typeof(object)),
                                        childParam),
                                    Expression.Call(
                                        Expression.Property(parentParam, child.Value.Column.Name),
                                        typeof(ICollection<>).MakeGenericType(
                                            child.Value.Column.Type.GetGenericArguments().First())
                                                             .GetMethod("Add"),
                                        new Expression[] { childParam }))));
                    statements.Add(ex);
                }
                else {
                    var ex = Expression.IfThen(
                        Expression.NotEqual(parentParam, Expression.Constant(null)),
                        Expression.Assign(Expression.Property(parentParam, child.Value.Column.Name), childParam));
                    statements.Add(ex);
                }

                // add each child node
                parameters.Add(childParam);
                this.VisitMultiCollectionTree<T>(
                    child.Value,
                    statements,
                    parameters,
                    otherDictionaryParam,
                    ref collectionFetchParamCounter);
            }
        }

        public Delegate GenerateNonCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            // this is simple, just take the arguments, map them
            // params go in order of fetch tree
            var tt = typeof(T);
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var rootParam = Expression.Parameter(rootType);
            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression> { rootParam };
            this.VisitNonCollectionTree<T>(fetchTree, statements, parameters);

            // add in the return statement and parameter
            statements.Add(rootParam);
            return Expression.Lambda(Expression.Block(statements), parameters).Compile();
        }

        private void VisitNonCollectionTree<T>(FetchNode fetchTree, ICollection<Expression> statements, ICollection<ParameterExpression> parameters) {
            var parentParam = parameters.Last();
            foreach (var child in fetchTree.Children) {
                // create a param
                var childParam = Expression.Parameter(this.generatedCodeManager.GetForeignKeyType(child.Value.Column.Type));

                // add the member assign expression, check for null first
                var ex = Expression.IfThen(
                    Expression.NotEqual(parentParam, Expression.Constant(null)),
                    Expression.Assign(Expression.Property(parentParam, child.Value.Column.Name), childParam));
                parameters.Add(childParam);
                statements.Add(ex);

                // add each child node
                this.VisitNonCollectionTree<T>(child.Value, statements, parameters);
            }
        }
    }
}