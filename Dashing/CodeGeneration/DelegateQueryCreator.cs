namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private readonly IGeneratedCodeManager generatedCodeManager;

        public DelegateQueryCreator(IGeneratedCodeManager codeManager) {
            this.dapperMapperGenerator = new DapperMapperGenerator(codeManager);
            this.generatedCodeManager = codeManager;
            this.trackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>> GetTrackingCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.trackingCollectionQueries.GetOrAdd(key, t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked));
            return ((Func<Delegate, Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>>)func)(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>> GetTrackingNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            if (this.trackingNoCollectionQueries == null) {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>> GetFKCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            var factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.foreignKeyCollectionQueries.GetOrAdd(key, t => this.GenerateForeignKeyCollection<T>(mapperFactory, isTracked));
            return (Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>> GetFKNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            if (this.foreignKeyNoCollectionQueries == null) {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private Delegate GenerateTrackingCollection<T>(Delegate mapperFactory, bool isTracked) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked);
        }

        private Delegate GenerateForeignKeyCollection<T>(Delegate mapperFactory, bool isTracked) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked);
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private Delegate GenerateCollectionFactory<T>(Type[] mapperParams, bool isTracked) {
            var tt = typeof(T);

            // Func<SelectWriterResult, SelectQuery<T>, IDbTransaction, IEnumerable<T>>
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(tt));
            var transactionParam = Expression.Parameter(typeof(IDbTransaction));

            // huh?
            var returnType = isTracked ? this.generatedCodeManager.GetTrackingType(tt) : this.generatedCodeManager.GetForeignKeyType(tt);
            var funcFactoryParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType), typeof(Delegate)));
            
            // Dictionary<object,T> dict = new Dictionary<object, T>();
            var dictionaryVariable = Expression.Variable(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            var dictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            var dictionaryExpr = Expression.Assign(dictionaryVariable, dictionaryInit);

            // Func<A,...,Z> mapper = (Func<A,...,Z>)funcFactory(dict)
            var mapperFuncType = typeof(Func<>).Assembly.DefinedTypes.First(m => m.Name == "Func`" + mapperParams.Count());
            var mapperType = mapperFuncType.MakeGenericType(mapperParams);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            var mapperExpr = Expression.Assign(mapperVariable, Expression.Convert(Expression.Invoke(funcFactoryParam, new Expression[] { dictionaryVariable }), mapperType));
            
            // var queryResult = SqlMapper.Query<...>(transaction.Connection, result.Sql, mapper, result.Parameters, transaction, buffer: true, splitOn: result.FetchTree, commandTimeout: int?, commandType: CommandType?);
            var queryResultVariable = Expression.Variable(typeof(IEnumerable<>).MakeGenericType(tt), "queryResult");
            var sqlMapperQuery = typeof(SqlMapper).GetMethods().First(m => m.Name == "Query" && m.GetGenericArguments().Count() == mapperParams.Count()).MakeGenericMethod(mapperParams);
            var queryExpr = Expression.Assign(
                queryResultVariable,
                Expression.Call(
                    sqlMapperQuery,
                    new Expression[] {
                        Expression.Property(transactionParam, "Connection"), 
                        Expression.Property(resultParam, "Sql"), 
                        mapperVariable, 
                        Expression.Property(resultParam, "Parameters"),
                        transactionParam,
                        Expression.Constant(true),
                        Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"),
                        Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(int))),
                        Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(CommandType)))
                    }));

            // return dict.Values;
            var returnDictValuesExpr = Expression.Property(dictionaryVariable, "Values");

            // funcFactory => ((results, sql, transaction) => /* above */ )
            var lambdaExpression =
                Expression.Lambda(
                    Expression.Lambda(
                        Expression.Block(
                            new[] { dictionaryVariable, mapperVariable, queryResultVariable },
                            new Expression[] { dictionaryExpr, mapperExpr, queryExpr, returnDictValuesExpr }),
                        resultParam,
                        queryParam,
                        transactionParam),
                    funcFactoryParam);
            return lambdaExpression.Compile();
        }
    }
}