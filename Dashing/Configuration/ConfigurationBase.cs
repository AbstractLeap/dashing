namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Engine;

    public abstract class ConfigurationBase : IConfiguration {
        private readonly ConnectionStringSettings connectionStringSettings;

        private readonly IMapper mapper;

        private readonly MethodInfo mapperMapForMethodInfo;

        private readonly IDictionary<Type, IMap> mappedTypes;

        private readonly ISessionFactory sessionFactory;

        private readonly ICodeGenerator codeGenerator;

        private readonly DbProviderFactory dbProviderFactory;

        private IGeneratedCodeManager codeManager;

        public IMapper Mapper {
            get {
                return this.mapper;
            }
        }

        public IEnumerable<IMap> Maps {
            get {
                return this.mappedTypes.Values;
            }
        }

        public IEngine Engine { get; set; }

        public IGeneratedCodeManager CodeManager {
            get {
                return this.codeManager ?? (this.codeManager = this.codeGenerator.Generate(this));
            }
        }

        public bool GetIsTrackedByDefault { get; set; }

        protected ConfigurationBase(IEngine engine, ConnectionStringSettings connectionStringSettings, DbProviderFactory dbProviderFactory, IMapper mapper, ISessionFactory sessionFactory, ICodeGenerator codeGenerator) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (connectionStringSettings == null) {
                throw new ArgumentNullException("connectionStringSettings");
            }

            if (dbProviderFactory == null) {
                throw new ArgumentNullException("dbProviderFactory");
            }

            if (mapper == null) {
                throw new ArgumentNullException("mapper");
            }

            if (sessionFactory == null) {
                throw new ArgumentNullException("sessionFactory");
            }

            if (codeGenerator == null) {
                throw new ArgumentNullException("codeGenerator");
            }

            this.Engine = engine;
            this.Engine.Configuration = this;
            this.connectionStringSettings = connectionStringSettings;
            this.dbProviderFactory = dbProviderFactory;
            this.mapper = mapper;
            this.sessionFactory = sessionFactory;
            this.codeGenerator = codeGenerator;

            this.mapperMapForMethodInfo = this.mapper.GetType().GetMethod("MapFor", new[] { typeof(Type) });
            this.mappedTypes = new Dictionary<Type, IMap>();
        }

        public IMap<T> GetMap<T>() {
            // TODO: check that the Map is indeed an IMap<T> or lift if it isn't
            var map = this.GetMap(typeof(T)) as IMap<T>;
            return map;
        }

        public IMap GetMap(Type type) {
            return ConfigurationHelper.GetMap(type, this.mappedTypes, this.codeGenerator.Configuration);
        }

        public bool HasMap(Type type) {
            return ConfigurationHelper.HasMap(type, this.mappedTypes, this.codeGenerator.Configuration);
        }

        private void Dirty() {
            this.codeManager = null;
        }

        public ISession BeginSession() {
            var connection = this.dbProviderFactory.CreateConnection();
            if (connection == null) {
                throw new InvalidOperationException("Could not create a connection using the supplied DbProviderFactory");
            }

            connection.ConnectionString = this.connectionStringSettings.ConnectionString;

            return this.sessionFactory.Create(this.Engine, connection);
        }

        public ISession BeginSession(IDbConnection connection) {
            return this.sessionFactory.Create(this.Engine, connection, disposeConnection: false);
        }

        public ISession BeginSession(IDbConnection connection, IDbTransaction transaction) {
            return this.sessionFactory.Create(this.Engine, connection, transaction, false);
        }

        protected IConfiguration Add<T>() {
            var type = typeof(T);
            if (!this.mappedTypes.ContainsKey(type)) {
                this.Dirty();
                var map = this.Mapper.MapFor<T>();
                map.Configuration = this;
                this.mappedTypes[type] = map;
            }

            return this;
        }

        protected IConfiguration Add(IEnumerable<Type> types) {
            this.Dirty();

            var maps =
                types.Distinct().Where(t => !this.mappedTypes.ContainsKey(t)).AsParallel().Select(t => this.mapperMapForMethodInfo.Invoke(this.mapper, new object[] { t }) as IMap);

            foreach (var map in maps) {
                // force into sequential
                map.Configuration = this;
                this.mappedTypes[map.Type] = map;
            }

            return this;
        }

        protected IConfiguration AddNamespaceOf<T>() {
            var type = typeof(T);
            var ns = type.Namespace;

            if (ns == null) {
                throw new ArgumentException("Namespace of the indicator type is null");
            }

            return this.Add(type.Assembly.GetTypes().Where(t => t.IsVisible && t.Namespace != null && t.Namespace.StartsWith(ns)));
        }

        protected IMap<T> Setup<T>() {
            this.Dirty();

            IMap map;
            IMap<T> mapt;
            var type = typeof(T);

            if (!this.mappedTypes.TryGetValue(type, out map)) {
                mapt = this.Mapper.MapFor<T>(); // just instantiate a Map<T> from scratch
                mapt.Configuration = this;
                this.mappedTypes[type] = mapt;
            }
            else {
                mapt = map as IMap<T>;

                if (mapt == null) {
                    this.mappedTypes[type] = mapt = Map<T>.From(map); // lift the Map into a Map<T>
                }
            }

            return mapt;
        }
    }
}