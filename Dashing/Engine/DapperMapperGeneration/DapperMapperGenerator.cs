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

        private readonly IConfiguration configuration;

        public DapperMapperGenerator(IConfiguration configuration) {
            this.configuration = configuration;
        }

        /// <summary>
        ///     Generates a Func for the passed in fetchTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="fetchTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        public Tuple<Delegate, Type[]> GenerateCollectionMapper<T>(FetchNode fetchTree) {
            // note that we can only fetch one collection at a time
            // so, if there's more than one in the SelectQuery they should be split out prior to calling this
            var tt = typeof(T);
            var currentRootParam = Expression.Parameter(tt, "currentRoot");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(tt), "results");

            //var dictionaryParam = Expression.Parameter(typeof(IDictionary<,>).MakeGenericType(typeof(object), rootType), "dict");
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(tt);
            var newRoot = Expression.Variable(typeof(bool), "newRoot");
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { tt };

            // var rootVar = (RootType)objects[0];
            GetRootAssignment<T>(statements, rootVar, objectsParam, tt);
            statements.Add(Expression.Call(rootVar, tt.GetMethod("EnableTracking")));

            // var newRoot = false;
            statements.Add(Expression.Assign(newRoot, Expression.Constant(false)));

            // check to see if rootVar different to currentRoot
            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { results.Add(rootVar); currentRootParam = rootVar; }
            var pkName = this.configuration.GetMap<T>().PrimaryKey.Name;
            statements.Add(Expression.IfThen(Expression.OrElse(Expression.Equal(currentRootParam, Expression.Constant(null)), Expression.NotEqual(Expression.Property(currentRootParam, pkName), Expression.Property(rootVar, pkName))),
                Expression.Block(
                    Expression.Call(resultsParam, typeof(ICollection<>).MakeGenericType(tt).GetMethod("Add"), rootVar),
                    Expression.Assign(currentRootParam, rootVar),
                    Expression.Assign(newRoot, Expression.Constant(true))
                )));

            var i = 1;
            statements.AddRange(this.VisitTree(fetchTree, currentRootParam, newRoot, objectsParam, false, false, mappedTypes, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            var innerLambda = Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar, newRoot }, statements), objectsParam);
            return Tuple.Create(Expression.Lambda(innerLambda, currentRootParam, resultsParam).Compile(), mappedTypes.ToArray());
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

                    mappedTypes.Add(childType);
                    var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                    var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                    var enableTrackingExpr = Expression.Call(Expression.Convert(arrayIndexExpr, typeof(ITrackedEntity)), typeof(ITrackedEntity).GetMethod("EnableTracking"));
                    var convertExpr = Expression.Convert(arrayIndexExpr, childType);
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
                                (Expression)Expression.IfThen(Expression.IsTrue(newRoot), assignRelationExpr);
                            break;
                        default:
                            throw new InvalidOperationException(
                                string.Format("Unexpected RelationshipType: {0}", childNode.Value.Column.Relationship));
                    }

                    // now visit the next fetch
                    ++i;
                    var innerStatements = this.VisitTree(childNode.Value, convertExpr, newRoot, objectsParam, visitedCollection, insideCollection || childNode.Value.Column.Relationship == RelationshipType.OneToMany, mappedTypes, ref i);
                    var thenExpr = new List<Expression> { enableTrackingExpr, ex };
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

        public Tuple<Delegate, Type[], Type[]> GenerateMultiCollectionMapper<T>(FetchNode fetchTree) {
            var tt = typeof(T);
            var currentRootParam = Expression.Parameter(tt, "currentRoot");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(tt), "results");

            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(tt, tt.Name);
            var newRoot = Expression.Variable(typeof(bool), "newRoot");
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { tt };
            var mapperClosureTypes = new List<Type>();
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
            GetRootAssignment<T>(statements, rootVar, objectsParam, tt);
            statements.Add(Expression.Call(rootVar, tt.GetMethod("EnableTracking")));

            // var newRoot = false;
            statements.Add(Expression.Assign(newRoot, Expression.Constant(false)));

            // check to see if rootVar different to currentRoot
            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { results.Add(rootVar); currentRootParam = rootVar; }
            var primaryKey = this.configuration.GetMap<T>().PrimaryKey;
            var pkName = primaryKey.Name;
            var currentRootPrimaryKeyExpr = Expression.Property(currentRootParam, pkName);
            statements.Add(Expression.IfThen(Expression.OrElse(Expression.Equal(currentRootParam, Expression.Constant(null)), Expression.NotEqual(currentRootPrimaryKeyExpr, Expression.Property(rootVar, pkName))),
                Expression.Block(
                    Expression.Call(resultsParam, typeof(ICollection<>).MakeGenericType(tt).GetMethod("Add"), rootVar),
                    Expression.Assign(currentRootParam, rootVar),
                    Expression.Assign(newRoot, Expression.Constant(true))
                )));


            int collectionFetchParamCounter = 0;
            var i = 1;
            statements.AddRange(this.VisitMultiCollectionTree<T>(fetchTree, ref collectionFetchParamCounter, currentRootParam, newRoot, primaryKey.Type, currentRootPrimaryKeyExpr, objectsParam, mappedTypes, mapperClosureTypes, collectionVariables, newVariables, ref i));

            // add in the return statement and parameter
            statements.Add(rootVar);
            var innerLambda = Expression.Lambda(Expression.Block(new ParameterExpression[] { rootVar, newRoot }.Union(newVariables), statements), objectsParam);
            return Tuple.Create(Expression.Lambda(innerLambda, new[] { currentRootParam, resultsParam }.Union(collectionVariables)).Compile(), mappedTypes.ToArray(), mapperClosureTypes.ToArray());
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        private IEnumerable<Expression> VisitMultiCollectionTree<T>(FetchNode node, ref int collectionFetchParamCounter, Expression parentExpression, ParameterExpression newRoot, Type newRootPrimaryKeyType, Expression newRootPrimaryKeyExpr, ParameterExpression objectsParam, IList<Type> mappedTypes, List<Type> mapperClosureTypes, List<ParameterExpression> collectionVariables, List<ParameterExpression> newVariables, ref int i) {
            var statements = new List<Expression>();
            foreach (var child in node.Children) {
                if (child.Value.IsFetched) {
                    // create a param
                    Type childType = child.Value.Column.Relationship == RelationshipType.OneToMany
                                         ? child.Value.Column.Type.GetGenericArguments().First()
                                         : child.Value.Column.Type;
                    mappedTypes.Add(childType);
                    var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(i));
                    var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                    var thisVar = Expression.Parameter(childType, childType.Name + i);
                    newVariables.Add(thisVar);
                    var convertExpr = Expression.Convert(arrayIndexExpr, childType);
                    var thisInit = Expression.Assign(thisVar, convertExpr);
                    var propExpr = Expression.Property(parentExpression, child.Value.Column.Name);

                    var thisChildNewRoot = newRoot;
                    var thisChildNewRootPrimaryKeyType = newRootPrimaryKeyType;
                    var thisChildNewRootPrimaryKeyExpr = newRootPrimaryKeyExpr;

                    // add the member assign expression
                    var thenExpr = new List<Expression> {
                        Expression.Call(Expression.Convert(arrayIndexExpr, typeof(ITrackedEntity)), typeof(ITrackedEntity).GetMethod("EnableTracking")),
                        thisInit };
                    if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                        // check dictionary for existing instance
                        var pk = child.Value.Column.ChildColumn.Map.PrimaryKey;
                        var pkType = pk.Type;
                        var collectionVariableIdx = mapperClosureTypes.Count / 2;
                        var primaryKeyProperty = Expression.Property(thisVar, pk.Name);
                        
                        // dictionary for storing entities by id for this type
                        var dictType = typeof(IDictionary<,>).MakeGenericType(pkType, childType);
                        mapperClosureTypes.Add(dictType);
                        var dictVar = Expression.Variable(dictType, "dict" + collectionVariableIdx);
                        collectionVariables.Add(dictVar);
                        
                        // Hashset<Tuple<ParentPkType, ChildPkType>> for indicating if the entity has been added to this entity
                        // tuple variable for checking whether this entity has been added to this collection
                        var tupleType = typeof(Tuple<,>).MakeGenericType(newRootPrimaryKeyType, pkType);
                        var tupleVar = Expression.Variable(tupleType, "tuple" + collectionVariableIdx);
                        var hashsetPairType = typeof(HashSet<>).MakeGenericType(tupleType);
                        mapperClosureTypes.Add(hashsetPairType);
                        var hashsetPairVar = Expression.Variable(hashsetPairType, "hashsetPair" + collectionVariableIdx);
                        newVariables.Add(tupleVar);
                        collectionVariables.Add(hashsetPairVar);
                        thenExpr.Add(Expression.Assign(tupleVar, Expression.New(tupleType.GetConstructor(new[] { newRootPrimaryKeyType, pkType }), newRootPrimaryKeyExpr, primaryKeyProperty)));

                        // variable for checking if the variable is new
                        var newCollectionEntryVar = Expression.Variable(typeof(bool), "new" + collectionVariableIdx);
                        thenExpr.Add(Expression.Assign(newCollectionEntryVar, Expression.Constant(false)));
                        newVariables.Add(newCollectionEntryVar);

                        // check for null and assign expr
                        var checkForNullAndAssignExpr =
                            Expression.Block(
                                Expression.IfThen(
                                    Expression.Equal(propExpr, Expression.Constant(null)),
                                    Expression.Assign(propExpr, Expression.New(typeof(List<>).MakeGenericType(childType)))),
                                Expression.Call(
                                    propExpr,
                                    typeof(ICollection<>).MakeGenericType(childType).GetMethod("Add"),
                                    new Expression[] { thisVar }),
                                Expression.Call(hashsetPairVar, hashsetPairType.GetMethod("Add"), tupleVar));


                        thenExpr.Add(
                            Expression.IfThenElse(
                                Expression.Call(
                                    dictVar,
                                    dictType.GetMethod("ContainsKey"),
                                    new Expression[] { primaryKeyProperty }),
                                    Expression.Block(
                                        Expression.Assign(thisVar, Expression.Property(dictVar, "Item", primaryKeyProperty)),
                                        Expression.IfThen(Expression.Not(Expression.Call(hashsetPairVar, hashsetPairType.GetMethod("Contains"), tupleVar)),
                                            checkForNullAndAssignExpr
                                        )
                                    )
                                ,
                                Expression.Block(
                                    Expression.Call(
                                        dictVar,
                                        dictType.GetMethods().First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                                        primaryKeyProperty,
                                        thisVar),
                                        checkForNullAndAssignExpr
                                    ,
                                    Expression.Assign(newCollectionEntryVar, Expression.Constant(true))
                                )
                            ));

                        // update newRoot parameters to reflect this collection as the new root
                        thisChildNewRootPrimaryKeyExpr = primaryKeyProperty;
                        thisChildNewRoot = newCollectionEntryVar;
                        thisChildNewRootPrimaryKeyType = pkType;
                    }
                    else {
                        thenExpr.Add(Expression.IfThen(Expression.IsTrue(newRoot), Expression.Assign(propExpr, thisVar)));
                    }

                    // add each child node
                    ++i;
                    var innerStatements = this.VisitMultiCollectionTree<T>(
                        child.Value,
                        ref collectionFetchParamCounter,
                        thisVar,
                        thisChildNewRoot,
                        thisChildNewRootPrimaryKeyType,
                        thisChildNewRootPrimaryKeyExpr,
                        objectsParam,
                        mappedTypes,
                        mapperClosureTypes,
                        collectionVariables,
                        newVariables,
                        ref i);
                    thenExpr.AddRange(innerStatements);
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
            }

            return statements;
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
            statements.Add(Expression.Call(rootVar, tt.GetMethod("EnableTracking")));

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
                    var mappedType = child.Value.Column.Type;
                    var assignExpr = Expression.Assign(propExpr, Expression.Convert(indexExpr, mappedType));
                    ++i;
                    mappedTypes.Add(mappedType);
                    var innerStatements = this.VisitNonCollectionTree<T>(child.Value, objectsParam, propExpr, ref i, mappedTypes);

                    var thenExpr = new List<Expression> {
                        Expression.Call(Expression.Convert(indexExpr, typeof(ITrackedEntity)), typeof(ITrackedEntity).GetMethod("EnableTracking")),
                                                            assignExpr
                                                        };
                    thenExpr.AddRange(innerStatements);
                    statements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                }
            }

            return statements;
        }
    }
}