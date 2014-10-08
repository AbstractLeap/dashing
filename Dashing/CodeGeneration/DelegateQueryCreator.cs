namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dapper;

    using Dashing.Engine;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.DML;

    internal class DelegateQueryCreator {
        private readonly DapperMapperGenerator dapperMapperGenerator;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncTrackingCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncForeignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncTrackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncForeignKeyNoCollectionQueries;

        private readonly IGeneratedCodeManager generatedCodeManager;

        public DelegateQueryCreator(IGeneratedCodeManager codeManager) {
            this.dapperMapperGenerator = new DapperMapperGenerator(codeManager);
            this.generatedCodeManager = codeManager;
            this.trackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncTrackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncForeignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncTrackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncForeignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, isTracked, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>> GetCollectionFunctionAsync<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapperFactory = this.GetCollectionFunction<T>(result, isTracked, true, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetNoCollectionFunction<T>(
            SelectWriterResult result,
            bool isTracked) {
            Delegate func;
            var mapper = this.GetNoCollectionFunction<T>(result, isTracked, false, out func);

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapper);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>> GetNoCollectionFunctionAsync<T>(
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
            Delegate mapper = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateNonCollectionMapper<T>(result.FetchTree, isTracked));
            func = queries.GetOrAdd(key, t => this.GenerateQuery<T>(mapper, isTracked, isAsync));
            return mapper;
        }

        private Delegate GetCollectionFunction<T>(SelectWriterResult result, bool isTracked, bool isAsync, out Delegate func) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var collectionQueries = isAsync ? (isTracked ? this.asyncTrackingCollectionQueries : this.asyncForeignKeyCollectionQueries) : (isTracked ? this.trackingCollectionQueries : this.foreignKeyCollectionQueries);

            Delegate mapperFactory;
            if (result.NumberCollectionsFetched == 1) {
                mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
                func = collectionQueries.GetOrAdd(key, t => this.GenerateCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, isAsync));
            }
            else {
                mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(result.FetchTree, isTracked));
                func = collectionQueries.GetOrAdd(key, t => this.GenerateCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, isAsync));
            }
            return mapperFactory;
        }

        private Delegate GenerateCollection<T>(Delegate mapperFactory, bool isTracked, int numberCollectionsFetched, bool isAsync) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked, numberCollectionsFetched, isAsync);
        }

        private Delegate GenerateQuery<T>(Delegate mapper, bool isTracked, bool isAsync) {
            var mapperParams = mapper.GetType().GetGenericArguments();
            var tt = typeof(T);

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));
            var mapperParam = Expression.Parameter(mapper.GetType());

            // call query function
            MethodInfo sqlMapperQuery;
            var queryResultVariable = GetQueryMethod<T>(mapperParams, isAsync, tt, out sqlMapperQuery);
            var callQueryExpression = GetCallQueryExpression<T>(sqlMapperQuery, connectionParam, resultParam, mapperParam, transactionParam);

            // generate lambda (mapper) => ((result, query, connection, transaction) => SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var expr = Expression.Lambda(
                        Expression.Lambda(
                            callQueryExpression,
                            resultParam,
                            queryParam,
                            connectionParam,
                            transactionParam),
                            mapperParam);
            return expr.Compile();
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private Delegate GenerateCollectionFactory<T>(Type[] mapperParams, bool isTracked, int numberCollectionFetches, bool isAsync) {
            var tt = typeof(T);
            var statements = new List<Expression>();
            var variableExpressions = new List<ParameterExpression>();

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));

            // huh?
            var returnType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var rootDictType = typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType);
            var otherDictType = typeof(IDictionary<,>).MakeGenericType(
                typeof(string),
                typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(object)));
            ParameterExpression funcFactoryParam;
            if (numberCollectionFetches > 1) {
                funcFactoryParam =
                    Expression.Parameter(
                        typeof(Func<,,>).MakeGenericType(
                            rootDictType,
                            otherDictType,
                            typeof(Delegate)));
            }
            else {
                funcFactoryParam =
                    Expression.Parameter(
                        typeof(Func<,>).MakeGenericType(
                            typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType),
                            typeof(Delegate)));
            }

            // Dictionary<object,T> dict = new Dictionary<object, T>();
            var dictionaryVariable = Expression.Variable(rootDictType);
            var dictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            var dictionaryExpr = Expression.Assign(dictionaryVariable, dictionaryInit);
            variableExpressions.Add(dictionaryVariable);
            statements.Add(dictionaryExpr);

            if (numberCollectionFetches > 1) {
                // Dictionary<string, IDictionary<object, object>
                var otherDictionaryVariable = Expression.Variable(otherDictType);
                var otherDictionaryInit =
                    Expression.New(
                        typeof(Dictionary<,>).MakeGenericType(otherDictType.GetGenericArguments()));
                var otherDictionaryExpr = Expression.Assign(
                    otherDictionaryVariable,
                    otherDictionaryInit);
                variableExpressions.Add(otherDictionaryVariable);
                statements.Add(otherDictionaryExpr);

                // now initialise inner dictionaries
                for (var i = 1; i <= numberCollectionFetches; ++i) {
                    var addExpr = Expression.Call(
                        otherDictionaryVariable,
                        otherDictType.GetMethods()
                                     .First(m => m.Name == "Add" && m.GetParameters().Count() == 2),
                        Expression.Constant("fetchParam_" + i),
                        Expression.New(
                            typeof(Dictionary<,>).MakeGenericType(typeof(object), typeof(object))));
                    statements.Add(addExpr);
                }
            }

            // Func<A,...,Z> mapper = (Func<A,...,Z>)funcFactory(dict)
            var mapperFuncType = typeof(Func<>).Assembly.DefinedTypes.First(m => m.Name == "Func`" + mapperParams.Count());
            var mapperType = mapperFuncType.MakeGenericType(mapperParams);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            BinaryExpression mapperExpr;
            if (numberCollectionFetches > 1) {
                mapperExpr = Expression.Assign(
                    mapperVariable,
                    Expression.Convert(
                        Expression.Invoke(
                            funcFactoryParam,
                            new Expression[] { dictionaryVariable, variableExpressions.ElementAt(1) }),
                        mapperType));
            }
            else {
                mapperExpr = Expression.Assign(
                    mapperVariable,
                    Expression.Convert(
                        Expression.Invoke(funcFactoryParam, new Expression[] { dictionaryVariable }),
                        mapperType));
            }

            variableExpressions.Add(mapperVariable);
            statements.Add(mapperExpr);

            // var queryResult = SqlMapper.Query<...>(connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            MethodInfo sqlMapperQuery;
            var queryResultVariable = GetQueryMethod<T>(mapperParams, isAsync, tt, out sqlMapperQuery);

            var queryExpr = Expression.Assign(
                queryResultVariable,
                GetCallQueryExpression<T>(sqlMapperQuery, connectionParam, resultParam, mapperVariable, transactionParam));
            variableExpressions.Add(queryResultVariable);
            statements.Add(queryExpr);

            // return dict.Values;
            var returnDictValuesExpr = Expression.Property(dictionaryVariable, "Values");
            statements.Add(returnDictValuesExpr);

            // funcFactory => ((results, sql, connection, transaction) => /* above */ )
            var lambdaExpression =
                Expression.Lambda(
                    Expression.Lambda(
                        Expression.Block(
                           variableExpressions,
                            statements),
                        resultParam,
                        queryParam,
                        connectionParam,
                        transactionParam),
                    funcFactoryParam);
            return lambdaExpression.Compile();
        }

        private static MethodCallExpression GetCallQueryExpression<T>(MethodInfo sqlMapperQuery, ParameterExpression connectionParam, ParameterExpression resultParam, ParameterExpression mapperVariable, ParameterExpression transactionParam) {
            return Expression.Call(
                sqlMapperQuery,
                new Expression[] {
                                     connectionParam, 
                                     Expression.Property(resultParam, "Sql"), 
                                     mapperVariable, 
                                     Expression.Property(resultParam, "Parameters"),
                                     transactionParam,
                                     Expression.Constant(true),
                                     Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"),
                                     Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(int))),
                                     Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(CommandType)))
                                 });
        }

        private static ParameterExpression GetQueryMethod<T>(Type[] mapperParams, bool isAsync, Type tt, out MethodInfo sqlMapperQuery) {
            ParameterExpression queryResultVariable = null;
            sqlMapperQuery = null;
            if (isAsync) {
                queryResultVariable = Expression.Variable(typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)), "queryResult");
                sqlMapperQuery =
                    typeof(SqlMapper).GetMethods()
                                     .First(m => m.Name == "QueryAsync" && m.GetGenericArguments().Count() == mapperParams.Count())
                                     .MakeGenericMethod(mapperParams);
            }
            else {
                queryResultVariable = Expression.Variable(typeof(IEnumerable<>).MakeGenericType(tt), "queryResult");
                sqlMapperQuery =
                    typeof(SqlMapper).GetMethods()
                                     .First(m => m.Name == "Query" && m.GetGenericArguments().Count() == mapperParams.Count())
                                     .MakeGenericMethod(mapperParams);
            }
            return queryResultVariable;
        }
    }
}