namespace Dashing.CodeGeneration {
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.DML;

    internal class DelegateQueryCreator {
        private readonly DapperMapperGenerator dapperMapperGenerator;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>> trackingMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>> foreignKeyMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>> multiCollectionTrackingMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>> multiCollectionForeignKeyMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncTrackingCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncForeignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncTrackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncForeignKeyNoCollectionQueries;

        private readonly IGeneratedCodeManager generatedCodeManager;

        private readonly IConfiguration configuration;

        public delegate FetchCollectionAwaiter<T> CollectionAsyncDelegate<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        public DelegateQueryCreator(IGeneratedCodeManager codeManager, IConfiguration configuration) {
            this.dapperMapperGenerator = new DapperMapperGenerator(codeManager, configuration);
            this.generatedCodeManager = codeManager;
            this.configuration = configuration;
            this.trackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>>();
            this.foreignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>>();
            this.multiCollectionTrackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>>();
            this.multiCollectionForeignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>>();
            this.trackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncTrackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncForeignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncTrackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncForeignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetCollectionFunction<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, isTracked, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        public CollectionAsyncDelegate<T> GetAsyncCollectionFunction<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, isTracked, true, out func);

            return (CollectionAsyncDelegate<T>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetNoCollectionFunction<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapper = this.GetNoCollectionFunction<T>(result, isTracked, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapper);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>> GetAsyncNoCollectionFunction<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapper = this.GetNoCollectionFunction<T>(result, isTracked, true, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>)func.DynamicInvoke(mapper);
        }

        private Delegate GetNoCollectionFunction<T>(SelectWriterResult result, bool isTracked, bool isAsync, out Delegate func) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var queries = isAsync
                              ? (isTracked ? this.asyncTrackingNoCollectionQueries : this.asyncForeignKeyNoCollectionQueries)
                              : (isTracked ? this.trackingNoCollectionQueries : this.foreignKeyNoCollectionQueries);
            var mapper = factoryDictionary.GetOrAdd(
                key,
                t => this.dapperMapperGenerator.GenerateNonCollectionMapper<T>(result.FetchTree, isTracked));
            func = queries.GetOrAdd(key, t => this.GenerateQuery<T>(mapper.Item1, isTracked, isAsync, mapper.Item2));
            return mapper.Item1;
        }

        private Delegate GetCollectionFunction<T>(SelectWriterResult result, bool isTracked, bool isAsync, out Delegate func) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var collectionQueries = isAsync
                                            ? (isTracked ? this.asyncTrackingCollectionQueries : this.asyncForeignKeyCollectionQueries)
                                            : (isTracked ? this.trackingCollectionQueries : this.foreignKeyCollectionQueries);
            if (result.NumberCollectionsFetched == 1) {
                var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
                Tuple<Delegate, Type[]> mapperFactory = factoryDictionary.GetOrAdd(
                        key,
                        t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
                func = collectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateCollectionFactory<T>(isTracked, result.NumberCollectionsFetched, isAsync, mapperFactory.Item2, null));


                return mapperFactory.Item1;
            }
            else {
                var factoryDictionary = isTracked ? this.multiCollectionTrackingMapperFactories : this.multiCollectionForeignKeyMapperFactories;

                Tuple<Delegate, Type[], Type[]> mapperFactory = factoryDictionary.GetOrAdd(
                        key,
                        t => this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(result.FetchTree, isTracked));
                func = collectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateCollectionFactory<T>(isTracked, result.NumberCollectionsFetched, isAsync, mapperFactory.Item2, mapperFactory.Item3));

                return mapperFactory.Item1;
            }
        }

        private Delegate GenerateQuery<T>(Delegate mapper, bool isTracked, bool isAsync, Type[] mappedTypes) {
            var tt = typeof(T);

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));
            var mapperParam = Expression.Parameter(mapper.GetType());

            // call query function
            var sqlMapperQuery = GetArbitraryQueryMethod<T>(tt, mappedTypes, isAsync);
            var callQueryExpression = GetArbitraryCallQueryExpression<T>(sqlMapperQuery, connectionParam, resultParam, mapperParam, transactionParam, mappedTypes);

            // generate lambda (mapper) => ((result, query, connection, transaction) => SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var expr = Expression.Lambda(
                Expression.Lambda(callQueryExpression, resultParam, queryParam, connectionParam, transactionParam),
                mapperParam);
            return expr.Compile();
        }

        private Expression GetArbitraryCallQueryExpression<T>(MethodInfo sqlMapperQuery, ParameterExpression connectionParam, ParameterExpression resultParam, ParameterExpression mapperParam, ParameterExpression transactionParam, Type[] mappedTypes) {
            return Expression.Call(
                sqlMapperQuery,
                new Expression[] {
                                     connectionParam, Expression.Property(resultParam, "Sql"),
                                     Expression.NewArrayInit(
                                         typeof(Type),
                                         mappedTypes.Select(t => Expression.Constant(t))),
                                     mapperParam,
                                     Expression.Property(resultParam, "Parameters"), transactionParam,
                                     Expression.Constant(true),
                                     Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"),
                                     Expression.Convert(
                                         Expression.Constant(null),
                                         typeof(Nullable<>).MakeGenericType(typeof(int))),
                                     Expression.Convert(
                                         Expression.Constant(null),
                                         typeof(Nullable<>).MakeGenericType(typeof(CommandType)))
                                 });
        }

        private MethodInfo GetArbitraryQueryMethod<T>(Type type, IEnumerable<Type> mappedTypes, bool isAsync) {
            return
                typeof(SqlMapper).GetMethods()
                                 .First(
                                     m =>
                                     m.Name == (isAsync ? "QueryAsync" : "Query")
                                     && m.GetParameters().Count() > 2
                                     && m.GetParameters().ElementAt(2).ParameterType
                                     == typeof(Type[])).MakeGenericMethod(type);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines",
            Justification = "This is hard to read the StyleCop way")]
        private Delegate GenerateCollectionFactory<T>(bool isTracked, int numberCollectionFetches, bool isAsync, Type[] mappedTypes, Type[] collectionTypes) {
            var tt = typeof(T);
            var statements = new List<Expression>();
            var variableExpressions = new List<ParameterExpression>();

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));

            // figure out the parameters for the middle func
            var returnType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var resultsType = typeof(IList<>).MakeGenericType(returnType);
            ParameterExpression funcFactoryParam;
            if (numberCollectionFetches == 1) {
                // Func<T, IList<T>, Delegate> first param is the current root, second param will contain the results
                funcFactoryParam = Expression.Parameter(typeof(Func<,,>).MakeGenericType(returnType, resultsType, typeof(Delegate)));
            }
            else if (numberCollectionFetches > 1) {
                if (collectionTypes == null) {
                    throw new ArgumentNullException("collectionTypes", "Can not be null if numberCollectionFetches > 1");
                }

                // Func<T, IList<T>, [IDictionary<Col1PkType, Col1Type>, .... ], typeof(Delegate)> first param is the current root, second param is results, 3rd to N-1 contains look ups for the collections
                var nonGenericFuncType = this.GetFuncTypeFor(3 + numberCollectionFetches);
                var typeParams = new List<Type> { returnType, resultsType };
                foreach (var type in collectionTypes) {
                    typeParams.Add(typeof(IDictionary<,>).MakeGenericType(this.configuration.GetMap(type).PrimaryKey.Type, type));
                }

                typeParams.Add(typeof(Delegate));
                funcFactoryParam = Expression.Parameter(nonGenericFuncType.MakeGenericType(typeParams.ToArray()));
            }
            else {
                throw new InvalidOperationException("Calling GenerateCollectionFactory on a query with no collections is invalid");
            }

            //var rootDictType = typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType);
            //var otherDictType = typeof(IDictionary<,>).MakeGenericType(
            //    typeof(int),
            //    typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object)));
            //if (numberCollectionFetches > 1) {
            //    funcFactoryParam = Expression.Parameter(typeof(Func<,,>).MakeGenericType(rootDictType, otherDictType, typeof(Delegate)));
            //}
            //else {
            //    funcFactoryParam =
            //        Expression.Parameter(
            //            typeof(Func<,>).MakeGenericType(typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType), typeof(Delegate)));
            //}

            // T currentRoot = null;
            var currentRootVarible = Expression.Variable(returnType, "currentRoot");
            var currentRootInit = Expression.Assign(currentRootVarible, Expression.Constant(null, returnType));
            variableExpressions.Add(currentRootVarible);
            statements.Add(currentRootInit);

            // IEnumerable<T> results = new List<T>();
            var resultVariable = Expression.Variable(typeof(IList<>).MakeGenericType(returnType), "results");
            var resultInit = Expression.New(typeof(List<>).MakeGenericType(returnType));
            var resultExpr = Expression.Assign(resultVariable, resultInit);
            variableExpressions.Add(resultVariable);
            statements.Add(resultExpr);

            // Dictionary<object,T> dict = new Dictionary<object, T>();
            //var dictionaryVariable = Expression.Variable(rootDictType);
            //var dictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            //var dictionaryExpr = Expression.Assign(dictionaryVariable, dictionaryInit);
            //variableExpressions.Add(dictionaryVariable);
            //statements.Add(dictionaryExpr);

            if (numberCollectionFetches > 1) {
                for (var i = 0; i < numberCollectionFetches; i++) {
                    // IDictionary<ColNPkType, ColNType> dictN = new Dictionary<ColNPkType, ColNType>();
                    var colType = collectionTypes[i];
                    var dictVariable = Expression.Variable(typeof(IDictionary<,>).MakeGenericType(this.configuration.GetMap(colType).PrimaryKey.Type, colType), "dict" + i);
                    var dictInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(this.configuration.GetMap(colType).PrimaryKey.Type, colType));
                    var dictAssign = Expression.Assign(dictVariable, dictInit);
                    variableExpressions.Add(dictVariable);
                    statements.Add(dictAssign);
                }

                // Dictionary<string, IDictionary<object, object>
                //var otherDictionaryVariable = Expression.Variable(otherDictType);
                //var otherDictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(otherDictType.GetGenericArguments()));
                //var otherDictionaryExpr = Expression.Assign(otherDictionaryVariable, otherDictionaryInit);
                //variableExpressions.Add(otherDictionaryVariable);
                //statements.Add(otherDictionaryExpr);

                //// now initialise inner dictionaries
                //for (var i = 1; i <= numberCollectionFetches; ++i) {
                //    var addExpr = Expression.Call(
                //        otherDictionaryVariable,
                //        otherDictType.GetMethods().First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                //        Expression.Constant(i),
                //        Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), typeof(object))));
                //    statements.Add(addExpr);
                //}
            }

            // Func<A,...,Z> mapper = (Func<A,...,Z>)funcFactory(dict)
            var mapperType = typeof(Func<,>).MakeGenericType(typeof(object[]), returnType);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            BinaryExpression mapperExpr = Expression.Assign(
                    mapperVariable,
                    Expression.Convert(
                        Expression.Invoke(funcFactoryParam, variableExpressions),
                        mapperType));
            
            variableExpressions.Add(mapperVariable);
            statements.Add(mapperExpr);

            // var queryResult = SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var sqlMapperQuery = this.GetArbitraryQueryMethod<T>(tt, mappedTypes, isAsync);

            if (!isAsync) {
                var queryExpr = GetArbitraryCallQueryExpression<T>(sqlMapperQuery, connectionParam, resultParam, mapperVariable, transactionParam, mappedTypes);
                statements.Add(queryExpr);

                // return results
                statements.Add(resultVariable);

                // return dict.Values;
                //var returnDictValuesExpr = Expression.Property(dictionaryVariable, "Values");
                //statements.Add(returnDictValuesExpr);

                // funcFactory => ((results, sql, connection, transaction) => /* above */ )
                var lambdaExpression =
                    Expression.Lambda(
                        Expression.Lambda(Expression.Block(variableExpressions, statements), resultParam, queryParam, connectionParam, transactionParam),
                        funcFactoryParam);
                return lambdaExpression.Compile();
            }
            else {
                var queryResultVariable = Expression.Variable(typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)));
                statements.Add(Expression.Assign(queryResultVariable, GetArbitraryCallQueryExpression<T>(sqlMapperQuery, connectionParam, resultParam, mapperVariable, transactionParam, mappedTypes)));
                variableExpressions.Add(queryResultVariable);

                // now add statements to create FetchCollectionAwaiter and return that
                var fetchCollectionAwaiterType = typeof(FetchCollectionAwaiter<>).MakeGenericType(tt);
                var awaiterResultVariable = Expression.Variable(fetchCollectionAwaiterType);
                variableExpressions.Add(awaiterResultVariable);
                statements.Add(Expression.Assign(awaiterResultVariable, Expression.New(fetchCollectionAwaiterType)));
                statements.Add(Expression.Assign(Expression.Property(awaiterResultVariable, "Awaiter"), Expression.Call(queryResultVariable, typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)).GetMethod("GetAwaiter"))));
                statements.Add(Expression.Assign(Expression.Property(awaiterResultVariable, "Results"), resultVariable));
                statements.Add(awaiterResultVariable);

                var lambdaExpression = Expression.Lambda(Expression.Lambda(typeof(CollectionAsyncDelegate<>).MakeGenericType(tt), Expression.Block(variableExpressions, statements), resultParam, queryParam, connectionParam, transactionParam), funcFactoryParam);
                return lambdaExpression.Compile();
            }
        }

        private Type GetFuncTypeFor(int numberParameters) {
            switch (numberParameters) {
                case 1:
                    return typeof(Func<>);

                case 2:
                    return typeof(Func<,>);

                case 3:
                    return typeof(Func<,,>);

                case 4:
                    return typeof(Func<,,,>);

                case 5:
                    return typeof(Func<,,,,>);

                case 6:
                    return typeof(Func<,,,,,>);

                case 7:
                    return typeof(Func<,,,,,,>);

                case 8:
                    return typeof(Func<,,,,,,,>);

                case 9:
                    return typeof(Func<,,,,,,,,>);

                case 10:
                    return typeof(Func<,,,,,,,,,>);

                case 11:
                    return typeof(Func<,,,,,,,,,,>);

                case 12:
                    return typeof(Func<,,,,,,,,,,,>);

                case 13:
                    return typeof(Func<,,,,,,,,,,,,>);

                case 14:
                    return typeof(Func<,,,,,,,,,,,,,>);

                case 15:
                    return typeof(Func<,,,,,,,,,,,,,,>);

                case 16:
                    return typeof(Func<,,,,,,,,,,,,,,,>);

                case 17:
                    return typeof(Func<,,,,,,,,,,,,,,,,>);

                default:
                    throw new InvalidOperationException(
                        "Dashing is unable to fetch more than 14 collections in one query due to that lack of Func<> overloads");
            }
        }
    }
}