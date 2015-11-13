namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DML;

    internal class MultiCollectionMapperGenerator : IMultiCollectionMapperGenerator {
        private readonly IConfiguration configuration;

        public MultiCollectionMapperGenerator(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public Tuple<Delegate, Type[], Type[]> GenerateMultiCollectionMapper<T>(FetchNode fetchTree) {
            var rootType = typeof(T);
            var currentRootParam = Expression.Parameter(rootType, "currentRoot");
            var rootVar = Expression.Variable(rootType, "rootVar");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(rootType), "results");
            var objectsParam = Expression.Parameter(typeof(object[]));
            var statements = new List<Expression>();
            var mappedTypes = new List<Type> { rootType };
            var mapperClosureTypes = new List<Type>();
            var collectionVariables = new List<ParameterExpression>();
            var newVariables = new List<ParameterExpression>();

            // var rootVar = (RootType)objects[0];
            statements.Add(Expression.Assign(rootVar, Expression.Convert(Expression.ArrayIndex(objectsParam, Expression.Constant(0)), rootType)));
            var primaryKey = this.configuration.GetMap<T>().PrimaryKey;
            var pkName = primaryKey.Name;
            var currentRootPrimaryKeyExpr = Expression.Property(currentRootParam, pkName);
            var objectParamArrayIdx = 1;
            var collectionFetchParamCounter = 0;
            var innerStatements = this.VisitMultiCollectionTree<T>(
                fetchTree,
                ref collectionFetchParamCounter,
                currentRootParam,
                primaryKey.Type,
                currentRootPrimaryKeyExpr,
                objectsParam,
                mappedTypes,
                mapperClosureTypes,
                collectionVariables,
                newVariables,
                ref objectParamArrayIdx);

            var newRootStatements = new List<Expression> {
                                                             Expression.Assign(currentRootParam, rootVar),
                                                             Expression.Call(
                                                                 resultsParam,
                                                                 typeof(ICollection<>).MakeGenericType(rootType).GetMethod("Add"),
                                                                 currentRootParam)
                                                         };
            newRootStatements.AddRange(innerStatements.Item1);
            newRootStatements.Add(Expression.Call(currentRootParam, rootType.GetMethod("EnableTracking")));

            // check to see if rootVar different to currentRoot
            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { results.Add(rootVar); currentRootParam = rootVar; }
            statements.Add(
                Expression.IfThen(
                    Expression.OrElse(
                        Expression.Equal(currentRootParam, Expression.Constant(null)),
                        Expression.NotEqual(currentRootPrimaryKeyExpr, Expression.Property(rootVar, pkName))),
                    Expression.Block(newRootStatements)));

            statements.AddRange(innerStatements.Item2);

            // add in the return statement and parameter
            statements.Add(rootVar);
            var innerLambda = Expression.Lambda(Expression.Block(new[] { rootVar }.Union(newVariables), statements), objectsParam);
            return Tuple.Create(
                Expression.Lambda(innerLambda, new[] { currentRootParam, resultsParam }.Union(collectionVariables)).Compile(),
                mappedTypes.ToArray(),
                mapperClosureTypes.ToArray());
        }

        /// <returns>
        ///     Item1 is the expressions for mapping the current root var, Item2 is all the other expressions from this branch
        /// </returns>
        private Tuple<IEnumerable<Expression>, IEnumerable<Expression>> VisitMultiCollectionTree<T>(
            FetchNode node,
            ref int collectionFetchParamCounter,
            Expression parentExpression,
            Type currentRootPrimaryKeyType,
            Expression currentRootPrimaryKeyExpr,
            ParameterExpression objectsParam,
            IList<Type> mappedTypes,
            List<Type> mapperClosureTypes,
            List<ParameterExpression> collectionVariables,
            List<ParameterExpression> newVariables,
            ref int objectParamArrayIdx) {
            var rootStatements = new List<Expression>();
            var collectionStatements = new List<Expression>();
            foreach (var child in node.Children) {
                if (child.Value.IsFetched) {
                    // create a param
                    var isOneToMany = child.Value.Column.Relationship == RelationshipType.OneToMany;
                    Type childType = isOneToMany ? child.Value.Column.Type.GetGenericArguments().First() : child.Value.Column.Type;
                    mappedTypes.Add(childType);

                    // common vars
                    var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(objectParamArrayIdx));
                    var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                    var thisVar = Expression.Parameter(childType, childType.Name + objectParamArrayIdx);
                    newVariables.Add(thisVar);
                    var convertExpr = Expression.Convert(arrayIndexExpr, childType);
                    var thisInit = Expression.Assign(thisVar, convertExpr);
                    var propExpr = Expression.Property(parentExpression, child.Value.Column.Name);

                    // if one to many then we start a new root
                    if (isOneToMany) {
                        var pk = child.Value.Column.ChildColumn.Map.PrimaryKey;
                        var pkType = pk.Type;
                        var collectionVariableIdx = mapperClosureTypes.Count / 2;
                        var pkExpr = Expression.Property(thisVar, pk.Name);

                        // dictionary for storing entities by id for this type
                        var dictType = typeof(IDictionary<,>).MakeGenericType(pkType, childType);
                        mapperClosureTypes.Add(dictType);
                        var dictVar = Expression.Variable(dictType, "dict" + collectionVariableIdx);
                        collectionVariables.Add(dictVar);

                        // Hashset<Tuple<ParentPkType, ChildPkType>> for indicating if the entity has been added to this entity
                        // tuple variable for checking whether this entity has been added to this collection
                        var tupleType = typeof(Tuple<,>).MakeGenericType(currentRootPrimaryKeyType, pkType);
                        var tupleVar = Expression.Variable(tupleType, "tuple" + collectionVariableIdx);
                        var hashsetPairType = typeof(HashSet<>).MakeGenericType(tupleType);
                        mapperClosureTypes.Add(hashsetPairType);
                        var hashsetPairVar = Expression.Variable(hashsetPairType, "hashsetPair" + collectionVariableIdx);
                        newVariables.Add(tupleVar);
                        collectionVariables.Add(hashsetPairVar);

                        ++objectParamArrayIdx;
                        var innerStatements = this.VisitMultiCollectionTree<T>(
                            child.Value,
                            ref collectionFetchParamCounter,
                            thisVar,
                            pkType,
                            pkExpr,
                            objectsParam,
                            mappedTypes,
                            mapperClosureTypes,
                            collectionVariables,
                            newVariables,
                            ref objectParamArrayIdx);

                        // instantiate the collection if necessary
                        rootStatements.Add(
                                Expression.IfThen(
                                    Expression.Equal(propExpr, Expression.Constant(null)),
                                    Expression.Assign(propExpr, Expression.New(typeof(List<>).MakeGenericType(childType)))));

                        // create the many to one code
                        var initRootExpr = Expression.IfThen(
                            ifExpr,
                            Expression.Block(
                                thisInit,
                                Expression.IfThenElse(
                                    Expression.Call(dictVar, dictType.GetMethod("ContainsKey"), new Expression[] { pkExpr }),
                                    Expression.Assign(thisVar, Expression.Property(dictVar, "Item", pkExpr)),
                                    Expression.Block(
                                        new[] {
                                            Expression.Call(
                                                      dictVar,
                                                      dictType.GetMethods().First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                                                      pkExpr,
                                                      thisVar)
                                              }.Union(innerStatements.Item1).Union(new[] { Expression.Call(thisVar, typeof(ITrackedEntity).GetMethod("EnableTracking")) }))),
                                Expression.Assign(
                                    tupleVar,
                                    Expression.New(
                                        tupleType.GetConstructor(new[] { currentRootPrimaryKeyType, pkType }),
                                        currentRootPrimaryKeyExpr,
                                        pkExpr)),
                                Expression.IfThen(
                                    Expression.Not(Expression.Call(hashsetPairVar, hashsetPairType.GetMethod("Contains"), tupleVar)),
                                    Expression.Block(
                                        Expression.Call(
                                            propExpr,
                                            typeof(ICollection<>).MakeGenericType(childType).GetMethod("Add"),
                                            new Expression[] { thisVar }),
                                        Expression.Call(hashsetPairVar, hashsetPairType.GetMethod("Add"), tupleVar)))));
                        collectionStatements.Add(initRootExpr);
                        collectionStatements.AddRange(innerStatements.Item2);
                    }
                    else {
                        ++objectParamArrayIdx;
                        var innerStatements = this.VisitMultiCollectionTree<T>(
                            child.Value,
                            ref collectionFetchParamCounter,
                            propExpr,
                            currentRootPrimaryKeyType,
                            currentRootPrimaryKeyExpr,
                            objectsParam,
                            mappedTypes,
                            mapperClosureTypes,
                            collectionVariables,
                            newVariables,
                            ref objectParamArrayIdx);

                        var thisStatements = new List<Expression> {
                                                                      thisInit,
                                                                      Expression.Assign(propExpr, thisVar)
                                                                  };
                        thisStatements.AddRange(innerStatements.Item1);
                        thisStatements.Add(Expression.Call(thisVar, typeof(ITrackedEntity).GetMethod("EnableTracking")));
                        var expr = Expression.IfThen(ifExpr, Expression.Block(thisStatements));
                        rootStatements.Add(expr);
                        collectionStatements.AddRange(innerStatements.Item2);
                    }
                }
            }

            return Tuple.Create<IEnumerable<Expression>, IEnumerable<Expression>>(rootStatements, collectionStatements);
        }
    }
}