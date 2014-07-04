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

    /// <summary>
    ///     The configuration base.
    /// </summary>
    public abstract class ConfigurationBase : IConfiguration {
        private readonly IEngine engine;

        private readonly string connectionString;

        private readonly IMapper mapper;

        private readonly MethodInfo mapperMapForMethodInfo;

        private readonly IDictionary<Type, IMap> mappedTypes;

        private readonly ISessionFactory sessionFactory;

        private readonly ICodeGenerator codeGenerator;

        private bool engineHasLatestMaps;

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

        public IEngine Engine {
            get {
                if (!this.engineHasLatestMaps) {
                    this.engine.UseMaps(this.mappedTypes);
                    this.engineHasLatestMaps = true;
                }

                return this.engine;
            }
        }

        public IGeneratedCodeManager CodeManager {
            get {
                return this.codeManager ?? (this.codeManager = this.codeGenerator.Generate(this));
            }
        }

        public bool GetIsTrackedByDefault { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigurationBase" /> class.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <param name="mapper">
        ///     The mapper.
        /// </param>
        /// <param name="sessionFactory">
        ///     The session factory.
        /// </param>
        /// <param name="codeGenerator">
        ///     The code generator
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        protected ConfigurationBase(IEngine engine, string connectionString, IMapper mapper, ISessionFactory sessionFactory, ICodeGenerator codeGenerator) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (connectionString == null) {
                throw new ArgumentNullException("connectionString");
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

            this.engine = engine;
            this.engine.Configuration = this;
            this.connectionString = connectionString;
            this.mapper = mapper;
            this.mapperMapForMethodInfo = mapper.GetType().GetMethod("MapFor", new[] { typeof(Type) });
            this.sessionFactory = sessionFactory;
            this.codeGenerator = codeGenerator;

            this.mappedTypes = new Dictionary<Type, IMap>();
        }

        public ConfigurationBase(ConnectionStringSettings connectionString, IMapper mapper, ISessionFactory sessionFactory, ICodeGenerator codeGenerator)
            : this(new EngineBase(new DialectFactory().Create(connectionString), DbProviderFactories.GetFactory(connectionString.ProviderName)), connectionString.ConnectionString, mapper, sessionFactory, codeGenerator)
        {
        }

        public IMap<T> GetMap<T>() {
            // TODO: check that the Map is indeed an IMap<T> or lift if it isn't
            var map = this.GetMap(typeof(T)) as IMap<T>;
            return map;
        }

        public IMap GetMap(Type type) {
            IMap map;

            if (!this.mappedTypes.TryGetValue(type, out map)) {
                throw new ArgumentException("That type is not mapped");
            }

            return map;
        }

        public bool HasMap(Type type) {
            return this.mappedTypes.ContainsKey(type);
        }

        private void Dirty() {
            this.engineHasLatestMaps = false;
            this.codeManager = null;
        }

        public ISession BeginSession() {
            return this.sessionFactory.Create(this.Engine.Open(this.connectionString), this);
        }

        /// <summary>
        ///     The begin session.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession BeginSession(IDbConnection connection) {
            return this.sessionFactory.Create(connection, this);
        }

        /// <summary>
        ///     The begin session.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="transaction">
        ///     The transaction.
        /// </param>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession BeginSession(IDbConnection connection, IDbTransaction transaction) {
            return this.sessionFactory.Create(connection, transaction, this);
        }

        /// <summary>
        ///     The add.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IConfiguration" />.
        /// </returns>
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

        /// <summary>
        ///     The add.
        /// </summary>
        /// <param name="types">
        ///     The types.
        /// </param>
        /// <returns>
        ///     The <see cref="IConfiguration" />.
        /// </returns>
        protected IConfiguration Add(IEnumerable<Type> types) {
            this.Dirty();

            var maps =
                types.Distinct()
                     .Where(t => !this.mappedTypes.ContainsKey(t))
                     .AsParallel()
                     .Select(t => this.mapperMapForMethodInfo.Invoke(this.mapper, new object[] { t }) as IMap);

            foreach (var map in maps) {
                // force into sequential
                map.Configuration = this;
                this.mappedTypes[map.Type] = map;
            }

            return this;
        }

        /// <summary>
        ///     The add namespace of.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IConfiguration" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        protected IConfiguration AddNamespaceOf<T>() {
            var type = typeof(T);
            var ns = type.Namespace;

            if (ns == null) {
                throw new ArgumentException("Namespace of the indicator type is null");
            }

            return this.Add(type.Assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(ns)));
        }

        /// <summary>
        ///     The setup.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
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