namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

    using Dapper;

    using Dashing.Engine;
    using Dashing.Engine.DapperMapperGeneration;

    internal class DelegateQueryCreator {
        private readonly DapperMapperGenerator dapperMapperGenerator;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyMapperFactories;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingCollectionQueries;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingNoCollectionQueries;

        private readonly ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private readonly IGeneratedCodeManager GeneratedCodeManager;

        public DelegateQueryCreator(IGeneratedCodeManager codeManager) {
            this.dapperMapperGenerator = new DapperMapperGenerator(codeManager);
            this.GeneratedCodeManager = codeManager;
            this.trackingMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyMapperFactories = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.trackingNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
            this.foreignKeyNoCollectionQueries = new ConcurrentDictionary<Tuple<Type, string>, Delegate>();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetTrackingCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            ConcurrentDictionary<Tuple<Type, string>, Delegate> factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.trackingCollectionQueries.GetOrAdd(key, t => this.GenerateTrackingCollection<T>(mapperFactory, isTracked));
            return ((Func<Delegate, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>>)func)(mapperFactory);
        }

        private Delegate GenerateTrackingCollection<T>(Delegate mapperFactory, bool isTracked) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked);
        }

        private Delegate GenerateCollectionFactory<T>(Type[] mapperParams, bool isTracked) {
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(typeof(T)));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var returnType = isTracked ? this.GeneratedCodeManager.GetTrackingType<T>() : this.GeneratedCodeManager.GetForeignKeyType<T>();
            var funcFactoryParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(typeof(IDictionary<,>).MakeGenericType(typeof(object), returnType), typeof(Delegate)));
            var dictionaryVariable = Expression.Variable(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            var dictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), returnType));
            var dictionaryExpr = Expression.Assign(dictionaryVariable, dictionaryInit);
            var mapperFuncType = typeof(Func<>).Assembly.DefinedTypes.First(m => m.Name == "Func`" + mapperParams.Count());
            var mapperType = mapperFuncType.MakeGenericType(mapperParams);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            var mapperExpr = Expression.Assign(mapperVariable, Expression.Convert(Expression.Invoke(funcFactoryParam, new Expression[] { dictionaryVariable }), mapperType));
            var queryResultVariable = Expression.Variable(typeof(IEnumerable<>).MakeGenericType(typeof(T)), "queryResult");
            var queryExpr = Expression.Assign(
                queryResultVariable,
                Expression.Call(
                    typeof(SqlMapper).GetMethods().First(m => m.Name == "Query" && m.GetGenericArguments().Count() == mapperParams.Count()).MakeGenericMethod(mapperParams),
                    new Expression[] {
                                         connectionParam, Expression.Property(resultParam, "Sql"), mapperVariable, Expression.Property(resultParam, "Parameters"),
                                         Expression.Convert(Expression.Constant(null), typeof(IDbTransaction)), Expression.Constant(true),
                                         Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"),
                                         Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(int))),
                                         Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(CommandType)))
                                     }));
            var returnDictValuesExpr = Expression.Property(dictionaryVariable, "Values");
            var lambdaExpression =
                Expression.Lambda(
                    Expression.Lambda(
                        Expression.Block(
                            new[] { dictionaryVariable, mapperVariable, queryResultVariable },
                            new Expression[] { dictionaryExpr, mapperExpr, queryExpr, returnDictValuesExpr }),
                        resultParam,
                        queryParam,
                        connectionParam),
                    funcFactoryParam);
            return lambdaExpression.Compile();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetTrackingNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            throw new NotImplementedException();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetFKCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            ConcurrentDictionary<Tuple<Type, string>, Delegate> factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var mapperFactory = factoryDictionary.GetOrAdd(key, t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.foreignKeyCollectionQueries.GetOrAdd(key, t => this.GenerateForeignKeyCollection<T>(mapperFactory, isTracked));
            return (Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>)func.DynamicInvoke(mapperFactory);
        }

        private Delegate GenerateForeignKeyCollection<T>(Delegate mapperFactory, bool isTracked) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            return this.GenerateCollectionFactory<T>(mapperParams, isTracked);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetFKNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            throw new NotImplementedException();
        }
    }
}