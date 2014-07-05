namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Extensions;

    internal class DapperMapperGenerator : IDapperMapperGenerator {
        private IGeneratedCodeManager generatedCodeManager;

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
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType<T>() : this.generatedCodeManager.GetForeignKeyType<T>();
            var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");
            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression> { Expression.Parameter(rootType) };
            this.AddDictionaryFetch<T>(dictionaryParam, fetchTree, statements, parameters, isTracked);
            this.VisitTree(fetchTree, statements, parameters, false);

            // add in the return statement and parameter
            statements.Add(parameters.First());
            return Expression.Lambda(Expression.Lambda(Expression.Block(statements), parameters), dictionaryParam).Compile();
        }

        private void AddDictionaryFetch<T>(ParameterExpression dictionaryParam, FetchNode fetchTree, IList<Expression> statements, IList<ParameterExpression> parameters, bool isTracked) {
            // primary key get expression
            var primaryKeyExpr = Expression.Convert(Expression.Property(parameters.First(), fetchTree.Children.First().Value.Column.Map.PrimaryKey.Name), typeof(object));

            // check the dictionary for this value
            var expr = Expression.Assign(
                parameters.First(),
                Expression.Call(
                    null,
                    typeof(DictionaryExtensions).GetMethods()
                                                .First(m => m.Name == "GetOrAdd" && m.GetParameters().Count() == 3 && m.GetParameters().Count(p => p.Name == "valueCreator") == 0)
                                                .MakeGenericMethod(typeof(object), isTracked ? this.generatedCodeManager.GetTrackingType<T>() : this.generatedCodeManager.GetForeignKeyType<T>()),
                    new Expression[] { dictionaryParam, primaryKeyExpr, parameters.First() }));
            statements.Add(expr);
        }

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
                    parameters.Add(childParam);
                    statements.Add(ex);
                }

                // add each child node
                parameters.Add(childParam);
                this.VisitTree(child.Value, statements, parameters, visitedCollection);
            }
        }

        public Delegate GenerateNonCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            // this is simple, just take the arguments, map them
            // params go in order of fetch tree
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType<T>() : this.generatedCodeManager.GetForeignKeyType<T>();
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