namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dapper;

    using Dashing.Engine.DML;

    public class GeneratedCodeManager : IGeneratedCodeManager {
        public CodeGeneratorConfig Config { get; private set; }

        public Assembly GeneratedCodeAssembly { get; private set; }

        private readonly IDictionary<Type, Type> foreignKeyTypes;

        private readonly IDictionary<Type, Type> trackingTypes;

        private readonly IDictionary<Type, Type> updateTypes;

        private readonly IDictionary<Type, Delegate> queryCalls;

        private readonly IDictionary<Type, Delegate> noFetchFkCalls;

        private readonly IDictionary<Type, Delegate> noFetchTrackingCalls;

        private delegate IEnumerable<T> DelegateQuery<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        private delegate T CreateUpdateClass<T>(Type type);

        private readonly IDictionary<Type, Delegate> updateCreators;

        private readonly Func<Type, string, bool> compileTimeFunctionExistsFunction;

        private readonly DelegateQueryCreator delegateQueryCreator;

        private delegate IEnumerable<T> NoFetchDelegate<out T>(IDbConnection conn, string sql, dynamic parameters, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        public GeneratedCodeManager(CodeGeneratorConfig config, Assembly generatedCodeAssembly) {
            this.Config = config;
            this.GeneratedCodeAssembly = generatedCodeAssembly;
            this.delegateQueryCreator = new DelegateQueryCreator(this);

            // generate function for checking which delegates exist
            this.compileTimeFunctionExistsFunction = this.GenerateExistsFunction();

            // go through the defined types and add them
            this.foreignKeyTypes = new Dictionary<Type, Type>();
            this.trackingTypes = new Dictionary<Type, Type>();
            this.updateTypes = new Dictionary<Type, Type>();
            this.queryCalls = new Dictionary<Type, Delegate>();
            this.noFetchFkCalls = new Dictionary<Type, Delegate>();
            this.noFetchTrackingCalls = new Dictionary<Type, Delegate>();
            this.updateCreators = new Dictionary<Type, Delegate>();

            foreach (var type in this.GeneratedCodeAssembly.DefinedTypes) {
                // find the base type from the users code
                if (type.Name.EndsWith(this.Config.ForeignKeyAccessClassSuffix)) {
                    this.foreignKeyTypes.Add(type.BaseType, type);

                    // add the queryCall for this base type
                    // compile dynamic expression for calling Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn)
                    // on the generated DapperWrapper
                    var parameters = new List<ParameterExpression> {
                        Expression.Parameter(typeof(SelectWriterResult), "result"), 
                        Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(type.BaseType), "query"), 
                        Expression.Parameter(typeof(IDbConnection), "connection"),
                        Expression.Parameter(typeof(IDbTransaction), "transaction")
                    };
                    var methodCallExpr = Expression.Call(this.GeneratedCodeAssembly.DefinedTypes.First(t => t.Name == "DapperWrapper").GetMethods().First(m => m.Name == "Query").MakeGenericMethod(type.BaseType), parameters);
                    var queryCall = Expression.Lambda(typeof(DelegateQuery<>).MakeGenericType(type.BaseType), methodCallExpr, parameters).Compile();
                    this.queryCalls.Add(type.BaseType, queryCall);

                    // add the query for no fetches but fk
                    this.MakeNoFetchCall(type, type.BaseType, this.noFetchFkCalls);
                }
                else if (type.Name.EndsWith(this.Config.TrackedClassSuffix)) {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                    this.MakeNoFetchCall(type, type.BaseType.BaseType, this.noFetchTrackingCalls);
                }
                else if (type.Name.EndsWith(this.Config.UpdateClassSuffix)) {
                    this.updateTypes.Add(type.BaseType, type);
                    this.updateCreators.Add(type.BaseType, this.MakeUpdateCreator(type));
                }
            }
        }

        private Func<Type, string, bool> GenerateExistsFunction() {
            var typeParam = Expression.Parameter(typeof(Type));
            var stringParam = Expression.Parameter(typeof(string));
            var typeDelegatesPropExpr = Expression.Field(null, this.GeneratedCodeAssembly.DefinedTypes.First(t => t.Name == "DapperWrapper").GetFields().First(p => p.Name == "TypeDelegates"));
            var indexExpr = Expression.Property(typeDelegatesPropExpr, typeDelegatesPropExpr.Type.GetProperty("Item"), new Expression[] { typeParam });
            var containsExpr = Expression.Call(indexExpr, typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate)).GetMethod("ContainsKey"), new Expression[] { stringParam });
            return (Func<Type, string, bool>)Expression.Lambda(containsExpr, typeParam, stringParam).Compile();
        }

        private Delegate MakeUpdateCreator(TypeInfo type) {
            var ctor = Expression.New(type);
            var returnType = typeof(CreateUpdateClass<>).MakeGenericType(type.BaseType);
            var parameters = Expression.Parameter(typeof(Type));
            var lambda = Expression.Lambda(returnType, ctor, parameters);
            return lambda.Compile();
        }

        private void MakeNoFetchCall(TypeInfo type, Type baseType, IDictionary<Type, Delegate> fetchCalls) {
            var noFetchParameters = new List<ParameterExpression> { Expression.Parameter(typeof(IDbConnection), "conn"), Expression.Parameter(typeof(string), "sql"), Expression.Parameter(typeof(object), "parameters"), Expression.Parameter(typeof(IDbTransaction), "tran"), Expression.Parameter(typeof(bool), "buffered"), Expression.Parameter(typeof(Nullable<>).MakeGenericType(typeof(int)), "commandTimeout"), Expression.Parameter(typeof(Nullable<>).MakeGenericType(typeof(CommandType)), "commandType") };
            var noFetchMethodCallExpr = Expression.Call(typeof(SqlMapper).GetMethods().First(m => m.Name == "Query" && m.IsGenericMethod).MakeGenericMethod(type), noFetchParameters);
            var noFetchQueryCall = Expression.Lambda(typeof(NoFetchDelegate<>).MakeGenericType(baseType), noFetchMethodCallExpr, noFetchParameters).Compile();
            fetchCalls.Add(baseType, noFetchQueryCall);
        }

        private IEnumerable<T> Tracked<T>(IEnumerable<T> rows) {
            foreach (var row in rows) {
                var trackedEntity = row as ITrackedEntity;
                trackedEntity.IsTracking = true;
                yield return row;
            }
        }

        public Type GetForeignKeyType(Type type) {
            return this.foreignKeyTypes[type];
        }

        public Type GetTrackingType(Type type) {
            return this.trackingTypes[type];
        }

        public Type GetUpdateType(Type type) {
            return this.updateTypes[type];
        }

        public T CreateForeignKeyInstance<T>() {
            return (T)Activator.CreateInstance(this.GetForeignKeyType(typeof(T)));
        }

        public T CreateTrackingInstance<T>() {
            return (T)Activator.CreateInstance(this.GetTrackingType(typeof(T)));
        }

        public T CreateUpdateInstance<T>() {
            return ((CreateUpdateClass<T>)this.updateCreators[typeof(T)])(typeof(T));
        }

        public void TrackInstance<T>(T entity) {
            ITrackedEntityInspector<T> inspector = new TrackedEntityInspector<T>(entity);
            inspector.ResumeTracking();
        }

        public IEnumerable<T> Query<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction) {
            if (query.HasFetches()) {
                // we've got a function generated by the CodeGenerator for this
                if (this.compileTimeFunctionExistsFunction(typeof(T), result.FetchTree.FetchSignature)) {
                    if (query.IsTracked) {
                        return this.Tracked(((DelegateQuery<T>)this.queryCalls[typeof(T)])(result, query, connection, transaction));
                    }

                    return ((DelegateQuery<T>)this.queryCalls[typeof(T)])(result, query, connection, transaction);
                }

                // otherwise, let's have a look in our local runtime cache
                // TODO support multiple collection fetches
                if (result.NumberCollectionsFetched > 0) {
                    if (query.IsTracked) {
                        return this.Tracked(this.delegateQueryCreator.GetTrackingCollectionFunction<T>(result, true)(result, query, connection, transaction));
                    }

                    return this.delegateQueryCreator.GetFKCollectionFunction<T>(result, false)(result, query, connection, transaction);
                }

                if (query.IsTracked) {
                    return this.Tracked(this.delegateQueryCreator.GetTrackingNoCollectionFunction<T>(result, true)(result, query, connection, transaction));
                }

                return this.delegateQueryCreator.GetFKNoCollectionFunction<T>(result, false)(result, query, connection, transaction);
            }

            if (query.IsTracked) {
                return this.Tracked(((NoFetchDelegate<T>)this.noFetchTrackingCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction));
            }

            return ((NoFetchDelegate<T>)this.noFetchFkCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction);
        }

        public IEnumerable<T> Query<T>(SqlWriterResult result, IDbConnection connection, IDbTransaction transaction, bool asTracked = false) {
            if (asTracked) {
                return this.Tracked(((NoFetchDelegate<T>)this.noFetchTrackingCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction));
            }

            return ((NoFetchDelegate<T>)this.noFetchFkCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction);
        }

        public IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null) {
            return connection.Query<T>(sql, new DynamicParameters(parameters), transaction);
        }

        public int Execute(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            return connection.Execute(sql, (object)param, transaction);
        }

        public T QueryScalar<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            return connection.Query<T>(sql, (object)param, transaction).SingleOrDefault();
        }
    }
}