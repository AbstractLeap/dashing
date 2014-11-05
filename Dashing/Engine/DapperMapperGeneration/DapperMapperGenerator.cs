namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        public Tuple<Delegate, Type[]> GenerateCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            // note that we can only fetch one collection at a time
            // so, if there's more than one in the SelectQuery they should be split out prior to calling this
            var tt = typeof(T);
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType);
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, rootType);

            // add dictionary fetch rootVar = dict.GetOrAdd((object)p.PostId, rootVar)
            this.AddDictionaryFetch<T>(dictionaryParam, fetchTree, statements, rootVar, isTracked);
            var i = 1;
            statements.AddRange(this.VisitTree(fetchTree, rootVar, objectsParam, false, mappedTypes, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar }, statements), objectsParam), dictionaryParam).Compile(), mappedTypes.ToArray());
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private void AddDictionaryFetch<T>(
            ParameterExpression dictionaryParam,
            FetchNode fetchTree,
            IList<Expression> statements,
            ParameterExpression rootVar,
            bool isTracked) {
            // primary key get expression
            var primaryKeyExpr = Expression.Convert(Expression.Property(rootVar, fetchTree.Children.First().Value.Column.Map.PrimaryKey.Name), typeof(object));

            // check the dictionary for this value
            // p => dict.GetOrAdd((object)p.PostId, p);
            var tt = typeof(T);
            var expr = Expression.Assign(
                rootVar,
                Expression.Call(
                    null,
                    typeof(DictionaryExtensions).GetMethods()
                                                .First(m => m.Name == "GetOrAdd" && m.GetParameters().Count() == 3 && m.GetParameters().Count(p => p.Name == "valueCreator") == 0)
                                                .MakeGenericMethod(
                                                    typeof(object),
                                                    isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt)),
                    new Expression[] { dictionaryParam, primaryKeyExpr, rootVar }));
            statements.Add(expr);
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private IEnumerable<Expression> VisitTree(FetchNode node, Expression parentExpression, ParameterExpression objectsParam, bool visitedCollection, IList<Type> mappedTypes, ref int i) {
            var statements = new List<Expression>();
            foreach (var childNode in node.Children) {
                // create a param
                Type childType;
                if (childNode.Value.Column.Relationship == RelationshipType.OneToMany) {
                    if (visitedCollection) {
                        throw new InvalidOperationException("You can only generate a mapper for one collection at a time");
                    }

                    childType = childNode.Value.Column.Type.GetGenericArguments().First();
                    visitedCollection = true;
                }
                else {
                    childType = childNode.Value.Column.Type;
                }

                var mappedType = this.generatedCodeManager.GetForeignKeyType(childType);
                mappedTypes.Add(mappedType);
                var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                var convertExpr = Expression.Convert(arrayIndexExpr, mappedType);
                var propExpr = Expression.Property(parentExpression, childNode.Value.Column.Name);

                Expression ex;
                switch (childNode.Value.Column.Relationship) {
                    case RelationshipType.OneToMany:
                        ex = InitialiseCollectionAndAddChild(propExpr, childNode.Value, convertExpr);
                        break;
                    case RelationshipType.ManyToOne:
                        ex = Expression.Assign(propExpr, convertExpr);
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected RelationshipType: {0}", childNode.Value.Column.Relationship));
                }

                // now visit the next fetch
                ++i;
                var innerStatements = this.VisitTree(childNode.Value, convertExpr, objectsParam, visitedCollection, mappedTypes, ref i);
                var thenExpr = new List<Expression> { ex };
                thenExpr.AddRange(innerStatements);
                statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
            }

            return statements;
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        private static Expression InitialiseCollectionAndAddChild(
            Expression propExpr,
            FetchNode childNode,
            Expression mappedType) {
            // potentially initialize and then add the member assign expression, check for null first
            return Expression.Block(
                // if the collection property is null, initialise it to a list
                    Expression.IfThen(
                // if (parent.Property == null) {
                        Expression.Equal(
                            propExpr,
                            Expression.Constant(null)),
                        Expression.Assign(
                // parent.Property = new List<T>();
                            propExpr,
                            Expression.New(
                                typeof(List<>).MakeGenericType(
                                    childNode.Column.Type.GetGenericArguments().First())))),
                        Expression.Call(
                // parent.Property.Add(child);
                            propExpr,
                            typeof(ICollection<>).MakeGenericType(
                                childNode.Column.Type.GetGenericArguments().First()).GetMethod("Add"),
                            new Expression[] { mappedType }));
        }

        public Tuple<Delegate, Type[]> GenerateMultiCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            var tt = typeof(T);
            var rootType = isTracked
                               ? this.generatedCodeManager.GetTrackingType(tt)
                               : this.generatedCodeManager.GetForeignKeyType(tt);
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType);
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };

            // root dictionary stores the root object
            var rootDictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");

            // other dictionary stores all the other collection objects
            var otherDictionaryParam =
                Expression.Parameter(
                    typeof(IDictionary<,>).MakeGenericType(
                        typeof(int),
                        typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object))));

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, rootType);

            this.AddDictionaryFetch<T>(rootDictionaryParam, fetchTree, statements, rootVar, isTracked);
            int collectionFetchParamCounter = 0;
            var i = 1;
            statements.AddRange(this.VisitMultiCollectionTree<T>(fetchTree, otherDictionaryParam, ref collectionFetchParamCounter, rootVar, objectsParam, mappedTypes, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar }, statements), objectsParam), rootDictionaryParam, otherDictionaryParam).Compile(), mappedTypes.ToArray());
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        private IEnumerable<Expression> VisitMultiCollectionTree<T>(FetchNode node, ParameterExpression otherDictionaryParam, ref int collectionFetchParamCounter, Expression parentExpression, ParameterExpression objectsParam, IList<Type> mappedTypes, ref int i) {
            var statements = new List<Expression>();
            foreach (var child in node.Children) {
                // create a param
                Type childType = child.Value.Column.Relationship == RelationshipType.OneToMany ? child.Value.Column.Type.GetGenericArguments().First() : child.Value.Column.Type;
                var mappedType = this.generatedCodeManager.GetForeignKeyType(childType);
                mappedTypes.Add(mappedType);
                var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                var convertExpr = Expression.Convert(arrayIndexExpr, mappedType);
                var propExpr = Expression.Property(parentExpression, child.Value.Column.Name);

                // add the member assign expression
                Expression ex = null;
                if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                    // check dictionary for existing instance
                    var dictAccessExpr = Expression.Property(
                        otherDictionaryParam,
                        "Item",
                        Expression.Constant(i));
                    var primaryKeyProperty = Expression.Property(
                        convertExpr,
                        child.Value.Column.ChildColumn.Map.PrimaryKey.Name);
                    var assignExpr = Expression.Assign(
                        Expression.ArrayAccess(objectsParam, Expression.Constant(i)),
                        Expression.Property(dictAccessExpr, "Item", Expression.Convert(primaryKeyProperty, typeof(object))));
                    ex = Expression.IfThenElse(
                                Expression.Call(
                                    dictAccessExpr,
                                    typeof(IDictionary<,>).MakeGenericType(
                                        typeof(object),
                                        typeof(object)).GetMethod("ContainsKey"),
                                    new Expression[] { Expression.Convert(primaryKeyProperty, typeof(object)) }),
                                assignExpr,
                                Expression.Block(
                                    Expression.Call(
                                        dictAccessExpr,
                                        typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object)).GetMethods()
                                                            .First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                                        Expression.Convert(primaryKeyProperty, typeof(object)),
                                        arrayIndexExpr),
                                    Expression.Call(
                                        propExpr,
                                        typeof(ICollection<>).MakeGenericType(
                                            child.Value.Column.Type.GetGenericArguments().First())
                                                             .GetMethod("Add"),
                                        new Expression[] { convertExpr })));
                }
                else {
                    ex = Expression.Assign(propExpr, convertExpr);
                }

                // add each child node
                ++i;
                var innerStatements = this.VisitMultiCollectionTree<T>(child.Value, otherDictionaryParam, ref collectionFetchParamCounter, convertExpr, objectsParam, mappedTypes, ref i);
                var thenExpr = new List<Expression> { ex };
                thenExpr.AddRange(innerStatements);
                statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
            }

            return statements;
        }

        public Tuple<Delegate, Type[]> GenerateNonCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            // this is simple, just take the arguments, map them
            // params go in order of fetch tree
            var tt = typeof(T);
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType);
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, rootType);

            // go through the tree
            int i = 1;
            statements.AddRange(this.VisitNonCollectionTree<T>(fetchTree, objectsParam, rootVar, ref i, mappedTypes));

            // add in the return statement and parameter
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar }, statements), objectsParam).Compile(), mappedTypes.ToArray());
        }

        private static void GetRootAssignment<T>(List<Expression> statements, ParameterExpression rootVar, ParameterExpression objectsParam, Type rootType) {
            statements.Add(Expression.Assign(rootVar, Expression.Convert(Expression.ArrayIndex(objectsParam, Expression.Constant(0)), rootType)));
        }

        private IEnumerable<Expression> VisitNonCollectionTree<T>(FetchNode fetchTree, ParameterExpression objectsParam, Expression parent, ref int i, IList<Type> mappedTypes) {
            var statements = new List<Expression>();
            foreach (var child in fetchTree.Children) {
                var propExpr = Expression.Property(parent, child.Value.Column.Name);
                var indexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                var ifExpr = Expression.NotEqual(indexExpr, Expression.Constant(null));
                var mappedType = this.generatedCodeManager.GetForeignKeyType(child.Value.Column.Type);
                var assignExpr = Expression.Assign(propExpr, Expression.Convert(indexExpr, mappedType));
                ++i;
                mappedTypes.Add(mappedType);
                var innerStatements = this.VisitNonCollectionTree<T>(
                    child.Value,
                    objectsParam,
                    propExpr,
                    ref i,
                    mappedTypes);

                var thenExpr = new List<Expression> { assignExpr };
                thenExpr.AddRange(innerStatements);
                statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
            }

            return statements;
        }
    }
}