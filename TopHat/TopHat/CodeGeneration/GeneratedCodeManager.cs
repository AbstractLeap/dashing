namespace TopHat.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dapper;

    using TopHat.Engine;

    public class GeneratedCodeManager : IGeneratedCodeManager {
        private Assembly generatedCodeAssembly;

        private IDictionary<Type, Type> foreignKeyTypes;

        private IDictionary<Type, Type> trackingTypes;

        private IDictionary<Type, Delegate> queryCalls;

        private IDictionary<Type, Delegate> noFetchFkCalls;

        private IDictionary<Type, Delegate> noFetchTrackingCalls;

        public CodeGeneratorConfig Config { get; private set; }

        private delegate IEnumerable<T> DelegateQuery<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn);

        private delegate IEnumerable<T> NoFetchDelegate<T>(IDbConnection conn, string sql, dynamic parameters, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        public Assembly GeneratedCodeAssembly {
            get {
                if (this.generatedCodeAssembly == null) {
                    throw new NullReferenceException("You must load the code before you can access the assembly");
                }

                return this.generatedCodeAssembly;
            }
        }

        public GeneratedCodeManager(CodeGeneratorConfig config) {
            this.Config = config;
        }

        public void LoadCode() {
            this.generatedCodeAssembly = Assembly.LoadFrom(this.Config.Namespace + ".dll");

            // go through the defined types and add them
            this.foreignKeyTypes = new Dictionary<Type, Type>();
            this.trackingTypes = new Dictionary<Type, Type>();
            this.queryCalls = new Dictionary<Type, Delegate>();
            this.noFetchFkCalls = new Dictionary<Type, Delegate>();
            this.noFetchTrackingCalls = new Dictionary<Type, Delegate>();

            foreach (var type in this.generatedCodeAssembly.DefinedTypes) {
                // find the base type from the users code
                if (type.Name.EndsWith(this.Config.ForeignKeyAccessClassSuffix))
                {
                    this.foreignKeyTypes.Add(type.BaseType, type);

                    // add the queryCall for this base type
                    // compile dynamic expression for calling Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn)
                    // on the generated DapperWrapper
                    var parameters = new List<ParameterExpression> {
                                                              Expression.Parameter(typeof(SqlWriterResult), "result"),
                                                              Expression.Parameter(typeof(SelectQuery<>).MakeGenericType(type.BaseType), "query"),
                                                              Expression.Parameter(typeof(IDbConnection), "conn")
                                                          };
                    var methodCallExpr =
                        Expression.Call(
                            this.generatedCodeAssembly.DefinedTypes.First(t => t.Name == "DapperWrapper")
                                .GetMethods()
                                .First(m => m.Name == "Query")
                                .MakeGenericMethod(type.BaseType),
                            parameters);
                    var queryCall =
                        Expression.Lambda(typeof(DelegateQuery<>).MakeGenericType(type.BaseType), methodCallExpr, parameters).Compile();
                    this.queryCalls.Add(type.BaseType, queryCall);

                    // add the query for no fetches but fk
                    this.MakeNoFetchCall(type, type.BaseType, this.noFetchFkCalls);
                }
                else if (type.Name.EndsWith(this.Config.TrackedClassSuffix))
                {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                    this.MakeNoFetchCall(type, type.BaseType.BaseType, this.noFetchTrackingCalls);
                }
            }
        }

        private void MakeNoFetchCall(TypeInfo type, Type baseType, IDictionary<Type, Delegate> fetchCalls) {
            var noFetchParameters = new List<ParameterExpression> {
                                                                      Expression.Parameter(typeof(IDbConnection), "conn"),
                                                                      Expression.Parameter(typeof(string), "sql"),
                                                                      Expression.Parameter(typeof(object), "parameters"),
                                                                      Expression.Parameter(typeof(IDbTransaction), "tran"),
                                                                      Expression.Parameter(typeof(bool), "buffered"),
                                                                      Expression.Parameter(typeof(Nullable<>).MakeGenericType(typeof(int)), "commandTimeout"),
                                                                      Expression.Parameter(typeof(Nullable<>).MakeGenericType(typeof(CommandType)), "commandType")
                                                                  };
            var noFetchMethodCallExpr = Expression.Call(typeof(SqlMapper).GetMethods().First(m => m.Name == "Query" && m.IsGenericMethod).MakeGenericMethod(type), noFetchParameters);
            var noFetchQueryCall = Expression.Lambda(typeof(NoFetchDelegate<>).MakeGenericType(baseType), noFetchMethodCallExpr, noFetchParameters).Compile();
            fetchCalls.Add(baseType, noFetchQueryCall);
        }

        public IEnumerable<T> Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn) {
            if (query.HasFetches()) {
                return ((DelegateQuery<T>)this.queryCalls[typeof(T)])(result, query, conn);
            }
            else {
                if (query.IsTracked) {
                    return ((NoFetchDelegate<T>)this.noFetchTrackingCalls[typeof(T)])(conn, result.Sql, result.Parameters);
                }
                else {
                    return ((NoFetchDelegate<T>)this.noFetchFkCalls[typeof(T)])(conn, result.Sql, result.Parameters);
                }
            }
        }

        public Type GetForeignKeyType<T>() {
            return this.foreignKeyTypes[typeof(T)];
        }

        public Type GetTrackingType<T>() {
            return this.trackingTypes[typeof(T)];
        }

        public T CreateForeignKeyInstance<T>() {
            return (T)Activator.CreateInstance(this.GetForeignKeyType<T>());
        }

        public T CreateTrackingInstance<T>() {
            return (T)Activator.CreateInstance(this.GetTrackingType<T>());
        }

        public void TrackInstance<T>(T entity) {
            ITrackedEntityInspector<T> inspector = new TrackedEntityInspector<T>(entity);
            inspector.ResumeTracking();
        }
    }
}