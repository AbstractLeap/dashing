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

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncTrackingCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> asyncForeignKeyCollectionQueries;

        private readonly IGeneratedCodeManager generatedCodeManager;

        public DelegateQueryCreator(IGeneratedCodeManager codeManager) {
            this.dapperMapperGenerator = new DapperMapperGenerator(codeManager);
            this.generatedCodeManager = codeManager;
            this.trackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncTrackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.asyncForeignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>  GetTrackingCollectionFunctionAsync<T>(
            SelectWriterResult result,
            bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;

            Delegate mapperFactory;
            Delegate func;
            if (result.NumberCollectionsFetched == 1) {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.asyncTrackingCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, true));
            }
            else {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.asyncTrackingCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, true));
            }

            return ((Func<Delegate, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>>)func)(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetTrackingCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;

            Delegate mapperFactory;
            Delegate func;
            if (result.NumberCollectionsFetched == 1) {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.trackingCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, false));
            }
            else {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.trackingCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, false));
            }

            return ((Func<Delegate, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>>)func)(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetTrackingNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            if (this.trackingNoCollectionQueries == null) {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>> GetFKCollectionFunctionAsync<T>(
            SelectWriterResult result,
            bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;

            Delegate mapperFactory;
            Delegate func;
            if (result.NumberCollectionsFetched == 1) {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.asyncForeignKeyCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateForeignKeyCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, true));
            }
            else {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.asyncForeignKeyCollectionQueries.GetOrAdd(
                    key,
                    t =>
                    this.GenerateForeignKeyCollection<T>(
                        mapperFactory,
                        isTracked,
                        result.NumberCollectionsFetched, true));
            }

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, Task<IEnumerable<T>>>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetFKCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;

            Delegate mapperFactory;
            Delegate func;
            if (result.NumberCollectionsFetched == 1) {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.foreignKeyCollectionQueries.GetOrAdd(
                    key,
                    t => this.GenerateForeignKeyCollection<T>(mapperFactory, isTracked, result.NumberCollectionsFetched, false));
            }
            else {
                mapperFactory = factoryDictionary.GetOrAdd(
                    key,
                    t =>
                    this.dapperMapperGenerator.GenerateMultiCollectionMapper<T>(
                        result.FetchTree,
                        isTracked));
                func = this.foreignKeyCollectionQueries.GetOrAdd(
                    key,
                    t =>
                    this.GenerateForeignKeyCollection<T>(
                        mapperFactory,
                        isTracked,
                        result.NumberCollectionsFetched,
                        false));
            }

            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> GetFKNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            if (this.foreignKeyNoCollectionQueries == null) {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private Delegate GenerateTrackingCollection<T>(Delegate mapperFactory, bool isTracked, int numberCollectionFetches, bool isAsync) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked, numberCollectionFetches, isAsync);
        }

        private Delegate GenerateForeignKeyCollection<T>(Delegate mapperFactory, bool isTracked, int numberCollectionFetches, bool isAsync) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked, numberCollectionFetches, isAsync);
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
                            new Expression[]
                            { dictionaryVariable, variableExpressions.ElementAt(1) }),
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
            ParameterExpression queryResultVariable = null;
            MethodInfo sqlMapperQuery = null;
            if (isAsync) {
                queryResultVariable = Expression.Variable(typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(tt)), "queryResult");
                sqlMapperQuery = typeof(SqlMapper).GetMethods().First(m => m.Name == "QueryAsync" && m.GetGenericArguments().Count() == mapperParams.Count()).MakeGenericMethod(mapperParams);
            }
            else {
                queryResultVariable = Expression.Variable(typeof(IEnumerable<>).MakeGenericType(tt), "queryResult");
                sqlMapperQuery = typeof(SqlMapper).GetMethods().First(m => m.Name == "Query" && m.GetGenericArguments().Count() == mapperParams.Count()).MakeGenericMethod(mapperParams);
            }

            var queryExpr = Expression.Assign(
                queryResultVariable,
                Expression.Call(
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
                    }));
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
    }
}