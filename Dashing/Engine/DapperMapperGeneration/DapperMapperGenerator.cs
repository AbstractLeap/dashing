using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Engine.DapperMapperGeneration {
    using System.Collections;
    using System.Data;
    using System.Linq.Expressions;

    using Dapper;

    internal class DapperMapperGenerator {


        /// <summary>
        /// Generates a Func for the passed in fetchTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="fetchTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        public Func<IDictionary<object, T>, Delegate> GenerateCollectionMapper<T>(FetchNode fetchTree) {
            // note that we can only fetch one collection at a time
            // so, if there's more than one in the SelectQuery they should be split out prior to calling this
            bool visitedCollection = false;
            var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(T)), "dict");
            var statements = new List<Expression>();
            var parameters = new List<ParameterExpression> { Expression.Parameter(typeof(T)) };
            this.AddDictionaryFetch<T>(dictionaryParam, fetchTree, statements, parameters);
            this.VisitTree(fetchTree, statements, parameters, false);

            // add in the return statement and parameter
            statements.Add(parameters.First());
            return (Func<IDictionary<object, T>, Delegate>)Expression.Lambda(Expression.Lambda(Expression.Block(statements), parameters), dictionaryParam).Compile();
        }

        private void AddDictionaryFetch<T>(ParameterExpression dictionaryParam, FetchNode fetchTree, IList<Expression> statements, IList<ParameterExpression> parameters) {
            // primary key get expression
            var primaryKeyExpr = Expression.Convert(Expression.Property(parameters.First(), fetchTree.Children.First().Value.Column.Map.PrimaryKey.Name), typeof(object));

            // check the dictionary for this value
            var expr = Expression.Assign(
                parameters.First(),
                Expression.Call(
                    null,
                    typeof(Dashing.Extensions.DictionaryExtensions).GetMethods()
                                                                   .First(
                                                                       m =>
                                                                       m.Name == "GetOrAdd" && m.GetParameters().Count() == 3
                                                                       && m.GetParameters().Count(p => p.Name == "valueCreator") == 0)
                                                                       .MakeGenericMethod(typeof(object), typeof(T)),
                    new Expression[] { dictionaryParam, primaryKeyExpr, parameters.First() }));
            statements.Add(expr);
        }

        private void VisitTree(FetchNode node, IList<Expression> statements, IList<ParameterExpression> parameters, bool visitedCollection) {
            var parentParam = parameters.Last();
            foreach (var child in node.Children) {
                // create a param
                ParameterExpression childParam;
                if (child.Value.Column.Relationship == Configuration.RelationshipType.OneToMany) {
                    if (visitedCollection) {
                        throw new InvalidOperationException("You can only generate a mapper for one collection at a time");
                    }

                    childParam = Expression.Parameter(child.Value.Column.Type.GetGenericArguments().First());
                    visitedCollection = true;
                }
                else {
                    childParam = Expression.Parameter(child.Value.Column.Type);
                }

                // add the member assign expression, check for null first
                if (child.Value.Column.Relationship == Configuration.RelationshipType.OneToMany) {
                    var ex = Expression.IfThen(
                        Expression.NotEqual(parentParam, Expression.Constant(null)),
                        Expression.Call(Expression.Property(parentParam, child.Value.Column.Name), typeof(ICollection<>).MakeGenericType(child.Value.Column.Type.GetGenericArguments().First()).GetMethod("Add"), new Expression[] { childParam }));
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

        public Delegate GenerateNonCollectionMapper<T>(FetchNode fetchTree) {
            // this is simple, just take the arguments, map them
            // params go in order of fetch tree
            var rootParam = Expression.Parameter(typeof(T));
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
                var childParam = Expression.Parameter(child.Value.Column.Type);

                // add the member assign expression, check for null first
                var ex = Expression.IfThen(Expression.NotEqual(parentParam, Expression.Constant(null)),
                    Expression.Assign(Expression.Property(parentParam, child.Value.Column.Name), childParam));
                parameters.Add(childParam);
                statements.Add(ex);

                // add each child node
                this.VisitNonCollectionTree<T>(child.Value, statements, parameters);
            }
        }
    }
}
