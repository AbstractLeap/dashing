namespace TopHat.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using TopHat.Engine;

    public class GeneratedCodeManager : IGeneratedCodeManager {
        private Assembly generatedCodeAssembly;

        private IDictionary<Type, Type> foreignKeyTypes;

        private IDictionary<Type, Type> trackingTypes;

        private IDictionary<Type, Delegate> queryCalls;

        private delegate IEnumerable<T> DelegateQuery<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn);

        public Assembly GeneratedCodeAssembly {
            get {
                if (this.generatedCodeAssembly == null) {
                    throw new NullReferenceException("You must load the code before you can access the assembly");
                }

                return this.generatedCodeAssembly;
            }
        }

        public void LoadCode(CodeGeneratorConfig config) {
            this.generatedCodeAssembly = Assembly.LoadFrom(config.Namespace + ".dll");

            // go through the defined types and add them
            this.foreignKeyTypes = new Dictionary<Type, Type>();
            this.trackingTypes = new Dictionary<Type, Type>();
            this.queryCalls = new Dictionary<Type, Delegate>();

            foreach (var type in this.generatedCodeAssembly.DefinedTypes) {
                // find the base type from the users code
                if (type.Name.EndsWith(config.ForeignKeyAccessClassSuffix)) {
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
                        Expression.Lambda(methodCallExpr, parameters).Compile();
                    this.queryCalls.Add(type.BaseType, queryCall);
                }
                else if (type.Name.EndsWith(config.TrackedClassSuffix)) {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                }
            }
        }

        public IEnumerable<T> Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn) {
            return ((DelegateQuery<T>)this.queryCalls[typeof(T)])(result, query, conn);
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