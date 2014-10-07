namespace Dashing.CodeGeneration {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dapper;

    using Dashing.Engine.DML;

    public class GeneratedCodeManager : IGeneratedCodeManager {
        public CodeGeneratorConfig Config { get; private set; }

        public Assembly GeneratedCodeAssembly { get; private set; }

        private readonly IDictionary<Type, Type> foreignKeyTypes;

        private readonly IDictionary<Type, Type> trackingTypes;

        private readonly IDictionary<Type, Type> updateTypes;

        private readonly IDictionary<Type, Delegate> queryCalls;

        private readonly IDictionary<Type, Delegate> asyncQueryCalls;

        private readonly IDictionary<Type, Delegate> noFetchFkCalls;

        private readonly IDictionary<Type, Delegate> noFetchTrackingCalls;

        private readonly IDictionary<Type, Delegate> asyncNoFetchFkCalls;

        private readonly IDictionary<Type, Delegate> asyncNoFetchTrackingCalls;

        private delegate IEnumerable<T> DelegateQuery<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        private delegate Task<IEnumerable<T>> DelegateQueryAsync<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        private delegate T CreateUpdateClass<T>(Type type);

        private readonly IDictionary<Type, Delegate> updateCreators;

        private readonly Func<Type, string, bool> compileTimeFunctionExistsFunction;

        private readonly DelegateQueryCreator delegateQueryCreator;

        private readonly IDictionary<Type, Delegate> addTrackingToInstanceDelegates;

        private delegate IEnumerable<T> NoFetchDelegate<out T>(IDbConnection conn, string sql, dynamic parameters, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        private delegate IEnumerableAwaiter<T> NoFetchDelegateAsync<T>(IDbConnection conn, string sql, dynamic parameters, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        private delegate T AddTrackingDelegate<T>(T entity);

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
            this.asyncQueryCalls = new Dictionary<Type, Delegate>();
            this.noFetchFkCalls = new Dictionary<Type, Delegate>();
            this.noFetchTrackingCalls = new Dictionary<Type, Delegate>();
            this.asyncNoFetchFkCalls = new Dictionary<Type, Delegate>();
            this.asyncNoFetchTrackingCalls = new Dictionary<Type, Delegate>();
            this.updateCreators = new Dictionary<Type, Delegate>();
            this.addTrackingToInstanceDelegates = new Dictionary<Type, Delegate>();

            foreach (var type in this.GeneratedCodeAssembly.DefinedTypes) {
                // find the base type from the users code
                if (type.Name.EndsWith(this.Config.ForeignKeyAccessClassSuffix)) {
                    this.foreignKeyTypes.Add(type.BaseType, type);

                    // add the queryCall for this base type
                    // compile dynamic expression for calling Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn)
                    // on the generated DapperWrapper
                    this.AddDapperWrapperQueryCall(type, "Query", this.queryCalls, typeof(DelegateQuery<>));
                    this.AddDapperWrapperQueryCall(type, "QueryAsync", this.asyncQueryCalls, typeof(DelegateQueryAsync<>));

                    // add the query for no fetches but fk
                    this.MakeNoFetchCall(type, type.BaseType, this.noFetchFkCalls, "Query", typeof(NoFetchDelegate<>));
                    this.MakeNoFetchCall(type, type.BaseType, this.asyncNoFetchFkCalls, "QueryAsync", typeof(NoFetchDelegateAsync<>));
                }
                else if (type.Name.EndsWith(this.Config.TrackedClassSuffix)) {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                    this.MakeNoFetchCall(type, type.BaseType.BaseType, this.noFetchTrackingCalls, "Query", typeof(NoFetchDelegate<>));
                    this.MakeNoFetchCall(type, type.BaseType.BaseType, this.asyncNoFetchTrackingCalls, "QueryAsync", typeof(NoFetchDelegateAsync<>));
                    this.MakeAddTrackingDelegate(type, type.BaseType.BaseType);
                }
                else if (type.Name.EndsWith(this.Config.UpdateClassSuffix)) {
                    this.updateTypes.Add(type.BaseType, type);
                    this.updateCreators.Add(type.BaseType, this.MakeUpdateCreator(type));
                }
            }
        }

        private void MakeAddTrackingDelegate(TypeInfo type, Type baseType) {
            var parameter = Expression.Parameter(baseType);
            var returnVar = Expression.Variable(type);

            // generate new tracked instance and map over properties
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(returnVar, Expression.New(type)));
            foreach (var property in baseType.GetProperties()) {
                expressions.Add(Expression.Assign(Expression.Property(returnVar, property.Name), Expression.Property(parameter, property.Name)));
            }

            // set istracked to true
            expressions.Add(Expression.Assign(Expression.Property(returnVar, "IsTracking"), Expression.Constant(true)));

            // return
            expressions.Add(returnVar);

            // add to dict
            this.addTrackingToInstanceDelegates.Add(
                baseType,
                Expression.Lambda(typeof(AddTrackingDelegate<>).MakeGenericType(baseType), Expression.Block(new[] { returnVar }, expressions), new[] { parameter }).Compile());
        }

        private void AddDapperWrapperQueryCall(TypeInfo type, string methodName, IDictionary<Type, Delegate> calls, Type delegateQueryType) {
            var parameters = new List<ParameterExpression> {
                                                               Expression.Parameter(
                                                                   typeof(SelectWriterResult),
                                                                   "result"),
                                                               Expression.Parameter(
                                                                   typeof(SelectQuery<>).MakeGenericType(
                                                                       type.BaseType),
                                                                   "query"),
                                                               Expression.Parameter(
                                                                   typeof(IDbConnection),
                                                                   "connection"),
                                                               Expression.Parameter(
                                                                   typeof(IDbTransaction),
                                                                   "transaction")
                                                           };
            var methodCallExpr =
                Expression.Call(
                    this.GeneratedCodeAssembly.DefinedTypes.First(t => t.Name == "DapperWrapper")
                        .GetMethods()
                        .First(m => m.Name == methodName)
                        .MakeGenericMethod(type.BaseType),
                    parameters);
            var queryCall =
                Expression.Lambda(
                    delegateQueryType.MakeGenericType(type.BaseType),
                    methodCallExpr,
                    parameters).Compile();
            calls.Add(type.BaseType, queryCall);
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

        private void MakeNoFetchCall(TypeInfo type, Type baseType, IDictionary<Type, Delegate> fetchCalls, string methodName, Type noFetchDelegateType) {
            var noFetchParameters = new List<ParameterExpression>();
            bool isAsync = false;
            if (noFetchDelegateType == typeof(NoFetchDelegateAsync<>)) {
                isAsync = true;
                noFetchParameters = new List<ParameterExpression> {
                                                                      Expression.Parameter(typeof(IDbConnection), "conn"),
                                                                      Expression.Parameter(typeof(string), "sql"),
                                                                      Expression.Parameter(typeof(object), "parameters"),
                                                                      Expression.Parameter(typeof(IDbTransaction), "tran"),
                                                                      Expression.Parameter(
                                                                          typeof(Nullable<>).MakeGenericType(typeof(int)),
                                                                          "commandTimeout"),
                                                                      Expression.Parameter(
                                                                          typeof(Nullable<>).MakeGenericType(typeof(CommandType)),
                                                                          "commandType")
                                                                  };
            }
            else {
                noFetchParameters = new List<ParameterExpression> {
                                                                      Expression.Parameter(typeof(IDbConnection), "conn"),
                                                                      Expression.Parameter(typeof(string), "sql"),
                                                                      Expression.Parameter(typeof(object), "parameters"),
                                                                      Expression.Parameter(typeof(IDbTransaction), "tran"),
                                                                      Expression.Parameter(typeof(bool), "buffered"),
                                                                      Expression.Parameter(
                                                                          typeof(Nullable<>).MakeGenericType(typeof(int)),
                                                                          "commandTimeout"),
                                                                      Expression.Parameter(
                                                                          typeof(Nullable<>).MakeGenericType(typeof(CommandType)),
                                                                          "commandType")
                                                                  };
            }

            var noFetchMethodCallExpr = Expression.Call(typeof(SqlMapper).GetMethods().First(m => m.Name == methodName && m.IsGenericMethod && m.GetParameters().Count() == noFetchParameters.Count).MakeGenericMethod(type), noFetchParameters);
            if (!isAsync) {
                var noFetchQueryCall =
                    Expression.Lambda(noFetchDelegateType.MakeGenericType(baseType), noFetchMethodCallExpr, noFetchParameters).Compile();
                fetchCalls.Add(baseType, noFetchQueryCall);
            }
            else {
                var wrapper = Expression.Variable(typeof(EnumerableAwaiter<>).MakeGenericType(type));
                var initWrapper = Expression.Assign(wrapper, Expression.New(wrapper.Type));
                var results = Expression.Assign(Expression.Property(wrapper, "Awaiter"), Expression.Call(noFetchMethodCallExpr, typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(type)).GetMethod("GetAwaiter")));
                var noFetchQueryCall =
                    Expression.Lambda(
                        noFetchDelegateType.MakeGenericType(baseType),
                        Expression.Block(new[] { wrapper }, new Expression[] { initWrapper, results, wrapper }),
                        noFetchParameters).Compile();
                fetchCalls.Add(baseType, noFetchQueryCall);
            }
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

        public T CreateTrackingInstance<T>(T entity) {
            if (entity is ITrackedEntity) {
                return entity;
            }

            return ((AddTrackingDelegate<T>)this.addTrackingToInstanceDelegates[typeof(T)])(entity);
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
            return connection.Query<T>(sql, (object)parameters, transaction);
        }

        public int Execute(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            return connection.Execute(sql, (object)param, transaction);
        }

        public T QueryScalar<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            return connection.Query<T>(sql, (object)param, transaction).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction) {
            if (query.HasFetches()) {
                // we've got a function generated by the CodeGenerator for this
                if (this.compileTimeFunctionExistsFunction(typeof(T), result.FetchTree.FetchSignature)) {
                    if (query.IsTracked) {
                        var results = await ((DelegateQueryAsync<T>)this.asyncQueryCalls[typeof(T)])(result, query, connection, transaction);
                        return this.Tracked(results);
                    }

                    return await ((DelegateQueryAsync<T>)this.asyncQueryCalls[typeof(T)])(result, query, connection, transaction);
                }

                // otherwise, let's have a look in our local runtime cache
                // TODO support multiple collection fetches
                if (result.NumberCollectionsFetched > 0) {
                    if (query.IsTracked) {
                        var results = await this.delegateQueryCreator.GetTrackingCollectionFunctionAsync<T>(result, true)(result, query, connection, transaction);
                        return this.Tracked(results);
                    }

                    return await this.delegateQueryCreator.GetFKCollectionFunctionAsync<T>(result, false)(result, query, connection, transaction);
                }

                if (query.IsTracked) {
                    var results = this.delegateQueryCreator.GetTrackingNoCollectionFunction<T>(result, true)(result, query, connection, transaction);
                    return this.Tracked(results);
                }

                return this.delegateQueryCreator.GetFKNoCollectionFunction<T>(result, false)(result, query, connection, transaction);
            }

            if (query.IsTracked) {
                var results = await ((NoFetchDelegateAsync<T>)this.asyncNoFetchTrackingCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction);
                return this.Tracked(results as IEnumerable<T>);
            }

            var asyncNoFetchFkResults = await ((NoFetchDelegateAsync<T>)this.asyncNoFetchFkCalls[typeof(T)])(connection, result.Sql, result.Parameters, transaction);
            return asyncNoFetchFkResults as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(
            SqlWriterResult sqlQuery,
            IDbConnection connection,
            IDbTransaction transaction,
            bool asTracked = false) {
            if (asTracked) {
                var results = await ((NoFetchDelegateAsync<T>)this.asyncNoFetchTrackingCalls[typeof(T)])(
                    connection,
                    sqlQuery.Sql,
                    sqlQuery.Parameters,
                    transaction);
                return this.Tracked(results as IEnumerable<T>);
            }

            var asyncNoFetchFkResults =
                await ((NoFetchDelegateAsync<T>)this.asyncNoFetchFkCalls[typeof(T)])(connection, sqlQuery.Sql, sqlQuery.Parameters, transaction);
            return asyncNoFetchFkResults as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null) {
            return await connection.QueryAsync<T>(sql, (object)parameters, transaction);
        }

        public async Task<int> ExecuteAsync(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            return await connection.ExecuteAsync(sql, (object)param, transaction);
        }

        public async Task<T> QueryScalarAsync<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            var results = await connection.QueryAsync<T>(sql, (object)param, transaction);
            return results.SingleOrDefault();
        }
    }
}