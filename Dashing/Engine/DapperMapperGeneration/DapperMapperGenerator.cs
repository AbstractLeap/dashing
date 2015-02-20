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

        private readonly IConfiguration configuration;

        public DapperMapperGenerator(IGeneratedCodeManager generatedCodeManager, IConfiguration configuration) {
            this.generatedCodeManager = generatedCodeManager;
            this.configuration = configuration;
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
            var currentRootParam = Expression.Parameter(rootType, "currentRoot");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(rootType), "results");

            //var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType);
            var newRoot = Expression.Variable(typeof(bool));
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, rootType);

            // var newRoot = false;
            statements.Add(Expression.Assign(newRoot, Expression.Constant(false)));

            // check to see if rootVar different to currentRoot
            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { results.Add(rootVar); currentRootParam = rootVar; }
            var pkName = this.configuration.GetMap<T>().PrimaryKey.Name;
            statements.Add(Expression.IfThen(Expression.OrElse(Expression.Equal(currentRootParam, Expression.Constant(null)), Expression.NotEqual(Expression.Property(currentRootParam, pkName), Expression.Property(rootVar, pkName))),
                Expression.Block(
                    Expression.Call(resultsParam, typeof(ICollection<>).MakeGenericType(rootType).GetMethod("Add"), rootVar),
                    Expression.Assign(currentRootParam, rootVar),
                    Expression.Assign(newRoot, Expression.Constant(true))
                )));

            var i = 1;
            statements.AddRange(this.VisitTree(fetchTree, currentRootParam, newRoot, objectsParam, false, false, mappedTypes, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar, newRoot }, statements), objectsParam), currentRootParam, resultsParam).Compile(), mappedTypes.ToArray());
        }

        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private IEnumerable<Expression> VisitTree(FetchNode node, Expression parentExpression, ParameterExpression newRoot, ParameterExpression objectsParam, bool visitedCollection, bool insideCollection, IList<Type> mappedTypes, ref int i) {
            var statements = new List<Expression>();
            foreach (var childNode in node.Children) {
                if (childNode.Value.IsFetched) {
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
                        case RelationshipType.OneToOne:
                            var assignRelationExpr = Expression.Assign(propExpr, convertExpr);
                            ex = insideCollection ? 
                                assignRelationExpr :
                                (Expression)Expression.IfThen(Expression.IsFalse(newRoot), assignRelationExpr);
                            break;
                        default:
                            throw new InvalidOperationException(
                                string.Format("Unexpected RelationshipType: {0}", childNode.Value.Column.Relationship));
                    }

                    // now visit the next fetch
                    ++i;
                    var innerStatements = this.VisitTree(childNode.Value, convertExpr, newRoot, objectsParam, visitedCollection, insideCollection || childNode.Value.Column.Relationship == RelationshipType.OneToMany, mappedTypes, ref i);
                    var thenExpr = new List<Expression> { ex };
                    thenExpr.AddRange(innerStatements);
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
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

        public Tuple<Delegate, Type[], Type[]> GenerateMultiCollectionMapper<T>(FetchNode fetchTree, bool isTracked) {
            var tt = typeof(T);
            var rootType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var currentRootParam = Expression.Parameter(rootType, "currentRoot");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(rootType), "results");

            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType);
            var newRoot = Expression.Variable(typeof(bool));
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };
            var collectionTypes = new List<Type>();
            var collectionVariables = new List<ParameterExpression>();
            var newVariables = new List<ParameterExpression>();

            //// root dictionary stores the root object
            //var rootDictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");

            //// other dictionary stores all the other collection objects
            //var otherDictionaryParam =
            //    Expression.Parameter(
            //        typeof(IDictionary<,>).MakeGenericType(
            //            typeof(int),
            //            typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object))));

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, rootType);

            // var newRoot = false;
            statements.Add(Expression.Assign(newRoot, Expression.Constant(false)));

            // check to see if rootVar different to currentRoot
            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { results.Add(rootVar); currentRootParam = rootVar; }
            var pkName = this.configuration.GetMap<T>().PrimaryKey.Name;
            statements.Add(Expression.IfThen(Expression.OrElse(Expression.Equal(currentRootParam, Expression.Constant(null)), Expression.NotEqual(Expression.Property(currentRootParam, pkName), Expression.Property(rootVar, pkName))),
                Expression.Block(
                    Expression.Call(resultsParam, typeof(ICollection<>).MakeGenericType(rootType).GetMethod("Add"), rootVar),
                    Expression.Assign(currentRootParam, rootVar),
                    Expression.Assign(newRoot, Expression.Constant(true))
                )));


            int collectionFetchParamCounter = 0;
            var i = 1;
            statements.AddRange(this.VisitMultiCollectionTree<T>(fetchTree, ref collectionFetchParamCounter, currentRootParam, newRoot, objectsParam, mappedTypes, collectionTypes, collectionVariables, newVariables, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            return Tuple.Create(Expression.Lambda(Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar, newRoot }.Union(newVariables), statements), objectsParam), new[] { currentRootParam, resultsParam }.Union(collectionVariables)).Compile(), mappedTypes.ToArray(), collectionTypes.ToArray());
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        private IEnumerable<Expression> VisitMultiCollectionTree<T>(FetchNode node, ref int collectionFetchParamCounter, Expression parentExpression, ParameterExpression newRoot, ParameterExpression objectsParam, IList<Type> mappedTypes, List<Type> collectionTypes, List<ParameterExpression> collectionVariables, List<ParameterExpression> newVariables, ref int i) {
            var statements = new List<Expression>();
            foreach (var child in node.Children) {
                if (child.Value.IsFetched) {
                    // create a param
                    Type childType = child.Value.Column.Relationship == RelationshipType.OneToMany
                                         ? child.Value.Column.Type.GetGenericArguments().First()
                                         : child.Value.Column.Type;
                    var mappedType = this.generatedCodeManager.GetForeignKeyType(childType);
                    mappedTypes.Add(mappedType);
                    var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                    var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                    var thisVar = Expression.Parameter(mappedType);
                    newVariables.Add(thisVar);
                    var convertExpr = Expression.Convert(arrayIndexExpr, mappedType);
                    var thisInit = Expression.Assign(thisVar, convertExpr);
                    var propExpr = Expression.Property(parentExpression, child.Value.Column.Name);

                    // add the member assign expression
                    Expression ex = null;
                    if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                        // check dictionary for existing instance
                        var pk = this.configuration.GetMap(childType).PrimaryKey;
                        var pkType = pk.Type;
                        collectionTypes.Add(mappedType);
                        var dictType = typeof(IDictionary<,>).MakeGenericType(pkType, mappedType);
                        var dictVar = Expression.Variable(dictType, "dict" + (collectionTypes.Count - 1));
                        collectionVariables.Add(dictVar);
                        //var dictAccessExpr = Expression.Property(otherDictionaryParam, "Item", Expression.Constant(i));
                        var primaryKeyProperty = Expression.Property(thisVar, pk.Name);
                        //var assignExpr = Expression.Assign(
                        //    Expression.ArrayAccess(objectsParam, Expression.Constant(i)),
                        //    Expression.Property(dictAccessExpr, "Item", Expression.Convert(primaryKeyProperty, typeof(object))));
                        ex =
                            Expression.IfThenElse(
                                Expression.Call(
                                    dictVar,
                                    dictType.GetMethod("ContainsKey"),
                                    new Expression[] { primaryKeyProperty }),
                                    Expression.Block(
                                    Expression.Assign(thisVar, Expression.Property(dictVar, "Item", primaryKeyProperty)),
                                    Expression.Assign(newRoot, Expression.Constant(false))
                                ),
                                Expression.Block(
                                    Expression.Call(
                                        dictVar,
                                        dictType.GetMethods().First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                                        primaryKeyProperty,
                                        thisVar),
                                    Expression.Call(
                                        propExpr,
                                        typeof(ICollection<>).MakeGenericType(childType).GetMethod("Add"),
                                        new Expression[] { thisVar }),
                                    Expression.Assign(newRoot, Expression.Constant(true))
                                )
                            );
                    }
                    else {
                        ex = Expression.IfThen(Expression.IsTrue(newRoot), Expression.Assign(propExpr, convertExpr));
                    }

                    // add each child node
                    ++i;
                    var innerStatements = this.VisitMultiCollectionTree<T>(
                        child.Value,
                        ref collectionFetchParamCounter,
                        thisVar,
                        newRoot,
                        objectsParam,
                        mappedTypes,
                        collectionTypes,
                        collectionVariables,
                        newVariables,
                        ref i);
                    var thenExpr = new List<Expression> { thisInit, ex };
                    thenExpr.AddRange(innerStatements);
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
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
                if (child.Value.IsFetched) {
                    var propExpr = Expression.Property(parent, child.Value.Column.Name);
                    var indexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                    var ifExpr = Expression.NotEqual(indexExpr, Expression.Constant(null));
                    var mappedType = this.generatedCodeManager.GetForeignKeyType(child.Value.Column.Type);
                    var assignExpr = Expression.Assign(propExpr, Expression.Convert(indexExpr, mappedType));
                    ++i;
                    mappedTypes.Add(mappedType);
                    var innerStatements = this.VisitNonCollectionTree<T>(child.Value, objectsParam, propExpr, ref i, mappedTypes);

                    var thenExpr = new List<Expression> { assignExpr };
                    thenExpr.AddRange(innerStatements);
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
            }

            return statements;
        }
    }
}