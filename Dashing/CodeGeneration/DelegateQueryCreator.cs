namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.DML;

    internal class DelegateQueryCreator {
        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>> mapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>> multiCollectionMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> collectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> noCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncNoCollectionQueries;

        private readonly IConfiguration configuration;

        private readonly NonCollectionMapperGenerator nonCollectionMapperGenerator;

        private readonly SingleCollectionMapperGenerator singleCollectionMapperGenerator;

        private readonly MultiCollectionMapperGenerator multiCollectionMapperGenerator;

        public delegate FetchCollectionAwaiter<T> CollectionAsyncDelegate<T>(
            SelectWriterResult result,
            SelectQuery<T> query,
            IDbConnection connection,
            IDbTransaction transaction) where T : class, new();

        public DelegateQueryCreator(IConfiguration configuration) {
            this.nonCollectionMapperGenerator = new NonCollectionMapperGenerator(configuration);
            this.singleCollectionMapperGenerator = new SingleCollectionMapperGenerator(configuration);
            this.multiCollectionMapperGenerator = new MultiCollectionMapperGenerator(configuration);
            this.configuration = configuration;
            this.mapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[]>>();
            this.multiCollectionMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Tuple<Delegate, Type[], Type[]>>();
            this.collectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.noCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetCollectionFunction<T>(
            SelectWriterResult result) where T : class, new() {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        public CollectionAsyncDelegate<T> GetAsyncCollectionFunction<T>(SelectWriterResult result) where T : class, new() {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, true, out func);

            return (CollectionAsyncDelegate<T>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetNoCollectionFunction<T>(
            SelectWriterResult result) where T : class, new() {
            Delegate func;
            var mapper = this.GetNoCollectionFunction<T>(result, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapper);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>> GetAsyncNoCollectionFunction<T>(
            SelectWriterResult result) where T : class, new() {
            Delegate func;
            var mapper = this.GetNoCollectionFunction<T>(result, true, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>)func.DynamicInvoke(mapper);
        }

        private Delegate GetNoCollectionFunction<T>(SelectWriterResult result, bool isAsync, out Delegate func) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var queries = isAsync ? this.asyncNoCollectionQueries : this.noCollectionQueries;
            var mapper = this.mapperFactories.GetOrAdd(key, t => this.nonCollectionMapperGenerator.GenerateNonCollectionMapper<T>(result.FetchTree));
            func = queries.GetOrAdd(key, t => this.GenerateQuery<T>(mapper.Item1, isAsync, mapper.Item2));
            return mapper.Item1;
        }

        private Delegate GetCollectionFunction<T>(SelectWriterResult result, bool isAsync, out Delegate func) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var collectionQueries = isAsync ? this.asyncCollectionQueries : this.collectionQueries;
            if (result.NumberCollectionsFetched == 1) {
                Tuple<Delegate, Type[]> mapperFactory = this.mapperFactories.GetOrAdd(
                    key,
                    t => this.singleCollectionMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree));
                func = collectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateCollectionFactory<T>(result.NumberCollectionsFetched, isAsync, mapperFactory.Item2, null));

                return mapperFactory.Item1;
            }
            else {
                Tuple<Delegate, Type[], Type[]> mapperFactory = this.multiCollectionMapperFactories.GetOrAdd(
                    key,
                    t => this.multiCollectionMapperGenerator.GenerateMultiCollectionMapper<T>(result.FetchTree));
                func = collectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateCollectionFactory<T>(result.NumberCollectionsFetched, isAsync, mapperFactory.Item2, mapperFactory.Item3));

                return mapperFactory.Item1;
            }
        }

        private Delegate GenerateQuery<T>(Delegate mapper, bool isAsync, Type[] mappedTypes) {
            var tt = typeof(T);

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));
            var mapperParam = Expression.Parameter(mapper.GetType());

            // call query function
            var sqlMapperQuery = GetArbitraryQueryMethod<T>(tt, mappedTypes, isAsync);
            var callQueryExpression = GetArbitraryCallQueryExpression<T>(
                sqlMapperQuery,
                connectionParam,
                resultParam,
                mapperParam,
                transactionParam,
                mappedTypes);

            // generate lambda (mapper) => ((result, query, connection, transaction) => SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var expr = Expression.Lambda(
                Expression.Lambda(callQueryExpression, resultParam, queryParam, connectionParam, transactionParam),
                mapperParam);
            return expr.Compile();
        }

        private Expression GetArbitraryCallQueryExpression<T>(
            MethodInfo sqlMapperQuery,
            ParameterExpression connectionParam,
            ParameterExpression resultParam,
            ParameterExpression mapperParam,
            ParameterExpression transactionParam,
            Type[] mappedTypes) {
            return Expression.Call(
                sqlMapperQuery,
                connectionParam,
                Expression.Property(resultParam, "Sql"),
                Expression.NewArrayInit(typeof(Type), mappedTypes.Select(t => Expression.Constant(t))),
                mapperParam,
                Expression.Property(resultParam, "Parameters"),
                transactionParam,
                Expression.Constant(true),
                Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"),
                Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(int))),
                Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(CommandType))));
        }

        private MethodInfo GetArbitraryQueryMethod<T>(Type type, IEnumerable<Type> mappedTypes, bool isAsync) {
            return
                typeof(SqlMapper).GetMethods()
                                 .First(
                                     m =>
                                     m.Name == (isAsync ? "QueryAsync" : "Query") && m.GetParameters().Count() > 2
                                     && m.GetParameters().ElementAt(2).ParameterType == typeof(Type[]))
                                 .MakeGenericMethod(type);
        }

        private Delegate GenerateCollectionFactory<T>(int numberCollectionFetches, bool isAsync, Type[] mappedTypes, Type[] mapperClosureTypes) {
            var tt = typeof(T);
            var statements = new List<Expression>();
            var variableExpressions = new List<ParameterExpression>();

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));

            // figure out the parameters for the middle func
            var resultsType = typeof(IList<>).MakeGenericType(tt);
            ParameterExpression funcFactoryParam;
            if (numberCollectionFetches == 1) {
                // Func<T, IList<T>, Delegate> first param is the current root, second param will contain the results
                funcFactoryParam = Expression.Parameter(typeof(Func<,,>).MakeGenericType(tt, resultsType, typeof(Delegate)));
            }
            else if (numberCollectionFetches > 1) {
                if (mapperClosureTypes == null) {
                    throw new ArgumentNullException("mapperClosureTypes", "Can not be null if numberCollectionFetches > 1");
                }

                // Func<T, IList<T>, [IDictionary<Col1PkType, Col1Type>, Hashset<Tuple<ParentPkType, Col1PkType>>, .... ], typeof(Delegate)> first param is the current root, second param is results, 3rd to N-1 contains look ups for the collections
                var nonGenericFuncType = this.GetFuncTypeFor(3 + (numberCollectionFetches * 2));
                var typeParams = new List<Type> { tt, resultsType };
                foreach (var type in mapperClosureTypes) {
                    typeParams.Add(type);
                }

                typeParams.Add(typeof(Delegate));
                funcFactoryParam = Expression.Parameter(nonGenericFuncType.MakeGenericType(typeParams.ToArray()));
            }
            else {
                throw new InvalidOperationException("Calling GenerateCollectionFactory on a query with no collections is invalid");
            }

            // T currentRoot = null;
            var currentRootVarible = Expression.Variable(tt, "currentRoot");
            var currentRootInit = Expression.Assign(currentRootVarible, Expression.Constant(null, tt));
            variableExpressions.Add(currentRootVarible);
            statements.Add(currentRootInit);

            // IEnumerable<T> results = new List<T>();
            var resultVariable = Expression.Variable(typeof(IList<>).MakeGenericType(tt), "results");
            var resultInit = Expression.New(typeof(List<>).MakeGenericType(tt));
            var resultExpr = Expression.Assign(resultVariable, resultInit);
            variableExpressions.Add(resultVariable);
            statements.Add(resultExpr);

            if (numberCollectionFetches > 1) {
                // add in all of the closure variables i.e. dict0, hashsetPair0
                var hashsetType = typeof(HashSet<>);
                var i = 0;
                foreach (var mapperClosureType in mapperClosureTypes) {
                    ParameterExpression closureVariable;
                    NewExpression closureInit;
                    if (mapperClosureType.GetGenericTypeDefinition() == hashsetType) {
                        closureVariable = Expression.Variable(mapperClosureType, "hashsetPair" + (i / 2));
                        closureInit = Expression.New(mapperClosureType);
                    }
                    else {
                        closureVariable = Expression.Variable(mapperClosureType, "dict" + (i / 2));
                        closureInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(mapperClosureType.GetGenericArguments()));
                    }

                    var closureAssign = Expression.Assign(closureVariable, closureInit);
                    variableExpressions.Add(closureVariable);
                    statements.Add(closureAssign);
                    i++;
                }
            }

            // Func<A,...,Z> mapper = (Func<A,...,Z>)funcFactory(dict)
            var mapperType = typeof(Func<,>).MakeGenericType(typeof(object[]), tt);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            BinaryExpression mapperExpr = Expression.Assign(
                mapperVariable,
                Expression.Convert(Expression.Invoke(funcFactoryParam, variableExpressions), mapperType));

            variableExpressions.Add(mapperVariable);
            statements.Add(mapperExpr);

            // var queryResult = SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var sqlMapperQuery = this.GetArbitraryQueryMethod<T>(tt, mappedTypes, isAsync);

            if (!isAsync) {
                var queryExpr = GetArbitraryCallQueryExpression<T>(
                    sqlMapperQuery,
                    connectionParam,
                    resultParam,
                    mapperVariable,
                    transactionParam,
                    mappedTypes);
                statements.Add(queryExpr);

                // return results
                statements.Add(resultVariable);

                // return dict.Values;
                //var returnDictValuesExpr = Expression.Property(dictionaryVariable, "Values");
                //statements.Add(returnDictValuesExpr);

                // funcFactory => ((results, sql, connection, transaction) => /* above */ )
                var lambdaExpression =
                    Expression.Lambda(
                        Expression.Lambda(
                            Expression.Block(variableExpressions, statements),
                            resultParam,
                            queryParam,
                            connectionParam,
                            transactionParam),
                        funcFactoryParam);
                return lambdaExpression.Compile();
            }
            else {
                var queryResultVariable = Expression.Variable(typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)));
                statements.Add(
                    Expression.Assign(
                        queryResultVariable,
                        GetArbitraryCallQueryExpression<T>(
                            sqlMapperQuery,
                            connectionParam,
                            resultParam,
                            mapperVariable,
                            transactionParam,
                            mappedTypes)));
                variableExpressions.Add(queryResultVariable);

                // now add statements to create FetchCollectionAwaiter and return that
                // TODO we don't need the FetchCollectionAwaiter any more??
                var fetchCollectionAwaiterType = typeof(FetchCollectionAwaiter<>).MakeGenericType(tt);
                var awaiterResultVariable = Expression.Variable(fetchCollectionAwaiterType);
                variableExpressions.Add(awaiterResultVariable);
                statements.Add(Expression.Assign(awaiterResultVariable, Expression.New(fetchCollectionAwaiterType)));
                statements.Add(
                    Expression.Assign(
                        Expression.Property(awaiterResultVariable, "Awaiter"),
                        Expression.Call(
                            queryResultVariable,
                            typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)).GetMethod("GetAwaiter"))));
                statements.Add(Expression.Assign(Expression.Property(awaiterResultVariable, "Results"), resultVariable));
                statements.Add(awaiterResultVariable);

                var lambdaExpression =
                    Expression.Lambda(
                        Expression.Lambda(
                            typeof(CollectionAsyncDelegate<>).MakeGenericType(tt),
                            Expression.Block(variableExpressions, statements),
                            resultParam,
                            queryParam,
                            connectionParam,
                            transactionParam),
                        funcFactoryParam);
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

        public DelegateProjectionResult<TProjection> GetProjectionResult<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query, FetchNode sqlResultFetchTree)
            where TBase : class, new() {
            throw new NotImplementedException();
        }
    }
}