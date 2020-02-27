namespace Dashing.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
#if NETSTANDARD2_0
    using System.Reflection;
#endif

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DML;

    internal class SingleCollectionMapperGenerator : ISingleCollectionMapperGenerator {
        private readonly IConfiguration configuration;

        public SingleCollectionMapperGenerator(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public Tuple<Delegate, Type[]> GenerateCollectionMapper<T>(FetchNode fetchTree) {
            var rootType = typeof(T);

            // closure variables
            var currentRootParam = Expression.Parameter(rootType, "currentRoot");
            var resultsParam = Expression.Parameter(typeof(IList<>).MakeGenericType(rootType), "results");

            // set up some stuff
            var objectsParam = Expression.Parameter(typeof(object[]));
            var rootVar = Expression.Variable(rootType, "root");
            var mappedTypes = new List<Type> { rootType };
            var statements = new List<Expression>();
            var newRootStatements = new List<Expression> {
                                                             Expression.Assign(currentRootParam, rootVar),
                                                             Expression.Call(
                                                                 resultsParam,
                                                                 typeof(ICollection<>).MakeGenericType(rootType).GetMethod("Add"),
                                                                 currentRootParam)
                                                         };
            var insideCollectionStatements = new List<Expression>();

            // assign root variable
            statements.Add(Expression.Assign(rootVar, Expression.Convert(Expression.ArrayIndex(objectsParam, Expression.Constant(0)), rootType)));

            // iterate through the tree
            var objectParamArrayIdx = 1;
            bool hasVisitedCollection = false;
            var innerStatements = this.VisitTree(
                fetchTree,
                currentRootParam,
                objectsParam,
                ref hasVisitedCollection,
                false,
                mappedTypes,
                ref objectParamArrayIdx);
            newRootStatements.AddRange(innerStatements.Item1);
            newRootStatements.Add(Expression.Call(currentRootParam, rootType.GetMethod("EnableTracking")));
            insideCollectionStatements.AddRange(innerStatements.Item2);

            // if currentRoomParam == null || currentRootParam.Pk != rootVar.Pk { currentRootParam = rootVar; currentRootParam.EnabledTracking(); results.Add(currentRootParam);  }
            var primaryKeyName = this.configuration.GetMap<T>().PrimaryKey.Name;
            statements.Add(
                Expression.IfThen(
                    Expression.OrElse(
                        Expression.Equal(currentRootParam, Expression.Constant(null)),
                        Expression.NotEqual(Expression.Property(currentRootParam, primaryKeyName), Expression.Property(rootVar, primaryKeyName))),
                    Expression.Block(newRootStatements)));

            statements.AddRange(insideCollectionStatements);
            statements.Add(rootVar);

            var innerLambda = Expression.Lambda(Expression.Block(new[] { rootVar }, statements), objectsParam);
            return Tuple.Create(Expression.Lambda(innerLambda, currentRootParam, resultsParam).Compile(), mappedTypes.ToArray());
        }

        private Tuple<IEnumerable<Expression>, IEnumerable<Expression>> VisitTree(
            FetchNode node,
            Expression currentBranchExpression,
            ParameterExpression objectsParam,
            ref bool hasVisitedCollection,
            bool isInsideCollection,
            List<Type> mappedTypes,
            ref int objectParamArrayIdx) {
            var newRootStatements = new List<Expression>();
            var insideCollectionStatements = new List<Expression>();
            foreach (var childNode in node.Children) {
                if (childNode.Value.IsFetched) {
                    // create a parameter
                    Type childType;
                    var isOneToMany = childNode.Value.Column.Relationship == RelationshipType.OneToMany;
                    if (isOneToMany) {
                        if (hasVisitedCollection) {
                            throw new InvalidOperationException("I only support a single collection fetch!");
                        }

                        childType = childNode.Value.Column.Type.GetGenericArguments().First();
                        hasVisitedCollection = true;
                    }
                    else {
                        childType = childNode.Value.Column.Type;
                    }

                    mappedTypes.Add(childType);
                    var arrayIndexExpr = Expression.ArrayIndex(objectsParam, Expression.Constant(objectParamArrayIdx));
                    var ifExpr = Expression.NotEqual(arrayIndexExpr, Expression.Constant(null));
                    var enableTrackingExpr = Expression.Call(
                        Expression.Convert(arrayIndexExpr, typeof(ITrackedEntity)),
                        typeof(ITrackedEntity).GetMethod("EnableTracking"));
                    var convertExpr = Expression.Convert(arrayIndexExpr, childType);
                    var propExpr = Expression.Property(currentBranchExpression, childNode.Value.Column.Name);

                    Expression ex;
                    switch (childNode.Value.Column.Relationship) {
                        case RelationshipType.OneToMany:
                            ex = Expression.IfThen(
                                Expression.Equal(propExpr, Expression.Constant(null)),
                                Expression.Assign(propExpr, Expression.New(typeof(List<>).MakeGenericType(childType))));
                            break;
                        case RelationshipType.ManyToOne:
                        case RelationshipType.OneToOne:
                            ex = Expression.Assign(propExpr, convertExpr);
                            break;
                        default:
                            throw new InvalidOperationException(
                                string.Format("Unexpected RelationshipType: {0}", childNode.Value.Column.Relationship));
                    }

                    // now visit the next fetch
                    ++objectParamArrayIdx;
                    var innerStatements = this.VisitTree(
                        childNode.Value,
                        isOneToMany ? (Expression)convertExpr : propExpr,
                        objectsParam,
                        ref hasVisitedCollection,
                        isInsideCollection || isOneToMany,
                        mappedTypes,
                        ref objectParamArrayIdx);
                    if (isInsideCollection) {
                        var thenExpr = new List<Expression> { ex };
                        thenExpr.AddRange(innerStatements.Item2);
                        thenExpr.Add(enableTrackingExpr);
                        insideCollectionStatements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                    }
                    else if (isOneToMany) {
                        var thenExpr = new List<Expression> { ex };
                        var topOfInsideCollectionStatements = new List<Expression> {
                                                                                       Expression.Call(
                                                                                           propExpr,
                                                                                           typeof(ICollection<>).MakeGenericType(childType)
                                                                                                                .GetMethod("Add"),
                                                                                           convertExpr)
                                                                                   };
                        topOfInsideCollectionStatements.AddRange(innerStatements.Item2);
                        topOfInsideCollectionStatements.Add(enableTrackingExpr);
                        insideCollectionStatements.Add(Expression.IfThen(ifExpr, Expression.Block(topOfInsideCollectionStatements)));
                        newRootStatements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                    }
                    else {
                        var thenExpr = new List<Expression> { ex };
                        thenExpr.AddRange(innerStatements.Item1);
                        thenExpr.Add(enableTrackingExpr);
                        newRootStatements.Add(Expression.IfThen(ifExpr, Expression.Block(thenExpr)));
                        insideCollectionStatements.AddRange(innerStatements.Item2);
                    }
                }
            }

            return new Tuple<IEnumerable<Expression>, IEnumerable<Expression>>(newRootStatements, insideCollectionStatements);
        }
    }
}