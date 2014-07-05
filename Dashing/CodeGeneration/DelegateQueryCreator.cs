using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.CodeGeneration {
    using System.Collections.Concurrent;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Engine;
    using Dashing.Engine.DapperMapperGeneration;

    internal class DelegateQueryCreator {
        private DapperMapperGenerator dapperMapperGenerator;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingMapperFactories;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyMapperFactories;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingCollectionQueries;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> trackingNoCollectionQueries;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyCollectionQueries;

        private ConcurrentDictionary<Tuple<Type, string>, Delegate> foreignKeyNoCollectionQueries;

        private IGeneratedCodeManager GeneratedCodeManager;

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
            var mapperFactory = (Func<IDictionary<object, T>, Delegate>)factoryDictionary.GetOrAdd(
                key,
                t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.trackingCollectionQueries.GetOrAdd(key, t => this.GenerateTrackingCollection<T>(mapperFactory));
            return ((Func<Func<IDictionary<object, T>, Delegate>, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>>)func)(mapperFactory);
        }

        private Func<Func<IDictionary<object, T>, Delegate>, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>> GenerateTrackingCollection<T>(Delegate mapperFactory) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            var queryTypeParams = mapperParams.Select(t => t == typeof(T) ? this.GeneratedCodeManager.GetTrackingType<T>() : this.GeneratedCodeManager.GetForeignKeyType(t)).ToArray();
            return GenerateCollectionFactory<T>(mapperParams, queryTypeParams);
        }

        private static Func<Func<IDictionary<object, T>, Delegate>, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>> GenerateCollectionFactory<T>(Type[] mapperParams, Type[] queryTypeParams) {
            var resultParam = Expression.Parameter(typeof(SelectWriterResult));
            var queryParam = Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(typeof(T)));
            var connectionParam = Expression.Parameter(typeof(IDbConnection));
            var funcFactoryParam = Expression.Parameter(typeof(Func<,>).MakeGenericType(typeof(IDictionary<,>).MakeGenericType(typeof(object), typeof(T)), typeof(Delegate)));
            var dictionaryInit = Expression.New(typeof(Dictionary<,>).MakeGenericType(typeof(object), typeof(T)));
            var mapperFuncType =
                typeof(System.Func<>).Assembly.DefinedTypes.First(
                    m => m.Name == "Func`" + queryTypeParams.Count());
            var mapperType = mapperFuncType.MakeGenericType(queryTypeParams);
            var mapperVariable = Expression.Variable(mapperType, "mapper");
            var mapperExpr = Expression.Assign(mapperVariable, Expression.Convert(Expression.Invoke(funcFactoryParam, new Expression[] { dictionaryInit }), mapperType));
            var expr =
                Expression.Call(
                    typeof(Dapper.SqlMapper).GetMethods().First(m => m.Name == "Query" && m.GetGenericArguments().Count() == mapperParams.Count()).MakeGenericMethod(queryTypeParams),
                    new Expression[] {
                                         connectionParam, Expression.Property(resultParam, "Sql"), mapperVariable, Expression.Property(resultParam, "Parameters"), Expression.Convert(Expression.Constant(null), typeof(IDbTransaction)),
                                         Expression.Constant(true), Expression.Property(Expression.Property(resultParam, "FetchTree"), "SplitOn"), Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(int))), Expression.Convert(Expression.Constant(null), typeof(Nullable<>).MakeGenericType(typeof(CommandType)))
                                     });
            var lambdaExpression = Expression.Lambda(Expression.Lambda(Expression.Block(new[] {mapperVariable}, new Expression[] {mapperExpr, expr}), resultParam, queryParam, connectionParam), funcFactoryParam);
            return
                (Func<Func<IDictionary<object, T>, Delegate>, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>>)
                lambdaExpression.Compile();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetTrackingNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            throw new NotImplementedException();
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetFKCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            var key = Tuple.Create(typeof(T), result.FetchTree.FetchSignature);
            ConcurrentDictionary<Tuple<Type, string>, Delegate> factoryDictionary = isTracked ? this.trackingMapperFactories : this.foreignKeyMapperFactories;
            var mapperFactory = (Func<IDictionary<object, T>, Delegate>)factoryDictionary.GetOrAdd(
                key,
                t => this.dapperMapperGenerator.GenerateCollectionMapper<T>(result.FetchTree, isTracked));
            var func = this.foreignKeyCollectionQueries.GetOrAdd(key, t => this.GenerateForeignKeyCollection<T>(mapperFactory));
            return ((Func<Func<IDictionary<object, T>, Delegate>, Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>>>)func)(mapperFactory);
        }

        private Delegate GenerateForeignKeyCollection<T>(Delegate mapperFactory) {
            var mapperParams = mapperFactory.GetType().GetGenericArguments().Last().GetGenericArguments();
            var queryTypeParams = mapperParams.Select(t => t == typeof(T) ? this.GeneratedCodeManager.GetForeignKeyType<T>() : this.GeneratedCodeManager.GetForeignKeyType(t)).ToArray();
            return GenerateCollectionFactory<T>(mapperParams, queryTypeParams);
        }

        public Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IEnumerable<T>> GetFKNoCollectionFunction<T>(SelectWriterResult result, bool isTracked) {
            throw new NotImplementedException();
        }
    }
}
