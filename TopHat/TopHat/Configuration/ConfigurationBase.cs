namespace TopHat.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using TopHat.CodeGeneration;
    using TopHat.Engine;

    /// <summary>
    ///     The configuration base.
    /// </summary>
    public abstract class ConfigurationBase : IConfiguration {
        /// <summary>
        ///     The _engine.
        /// </summary>
        private IEngine engine;

        /// <summary>
        ///     The _connection string.
        /// </summary>
        private readonly string connectionString;

        private IGeneratedCodeManager codeManager;

        private IMapper mapper;

        private MethodInfo mapperMapForMethodInfo;

        /// <summary>
        ///     Gets or sets the mapper.
        /// </summary>
        protected IMapper Mapper {
            get {
                return this.mapper;
            }

            set {
                this.mapper = value;
                this.mapperMapForMethodInfo = this.mapper.GetType().GetMethod("MapFor");
            }
        }

        /// <summary>
        ///     Gets or sets the session factory.
        /// </summary>
        protected ISessionFactory SessionFactory { get; set; }

        /// <summary>
        ///     Gets or sets the mapped types.
        /// </summary>
        protected IDictionary<Type, IMap> MappedTypes { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether engine has latest maps.
        /// </summary>
        protected bool EngineHasLatestMaps { get; set; }

        public IMap GetMap<T>() {
            return this.GetMap(typeof(T));
        }

        public IMap GetMap(Type type) {
            if (!this.MappedTypes.ContainsKey(type)) {
                throw new ArgumentException("That type is not mapped");
            }

            return this.MappedTypes[type];
        }

        /// <summary>
        ///     Gets the maps.
        /// </summary>
        public IEnumerable<IMap> Maps {
            get {
                return this.MappedTypes.Values;
            }
        }

        /// <summary>
        ///     Gets or sets the engine.
        /// </summary>
        protected IEngine Engine {
            get {
                if (!this.EngineHasLatestMaps) {
                    this.engine.UseMaps(this.MappedTypes);
                    this.EngineHasLatestMaps = true;
                }

                return this.engine;
            }

            set {
                this.Dirty();
                this.engine = value;
            }
        }

        /// <summary>
        ///     The dirty.
        /// </summary>
        private void Dirty() {
            this.EngineHasLatestMaps = false;
        }

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
        /// <exception cref="ArgumentNullException">
        /// </exception>
        protected ConfigurationBase(IEngine engine, string connectionString, IMapper mapper, ISessionFactory sessionFactory, IGeneratedCodeManager codeManager) {
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

            if (codeManager == null) {
                throw new ArgumentNullException("codeManager");
            }

            this.engine = engine;
            this.engine.Configuration = this;
            this.connectionString = connectionString;
            this.Mapper = mapper;
            this.SessionFactory = sessionFactory;
            this.MappedTypes = new Dictionary<Type, IMap>();
            this.codeManager = codeManager;

            // TODO: allow overriding of the CodeGeneratorConfig
            this.codeManager.LoadCode();
        }

        /// <summary>
        ///     The begin session.
        /// </summary>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession BeginSession() {
            return this.SessionFactory.Create(this.Engine.Open(this.connectionString), this);
        }

        public IGeneratedCodeManager GetCodeManager() {
            return this.codeManager;
        }

        public IEngine GetEngine() {
            return this.engine;
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
            return this.SessionFactory.Create(connection, this);
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
            return this.SessionFactory.Create(connection, transaction, this);
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
            if (!this.MappedTypes.ContainsKey(type)) {
                this.Dirty();
                this.MappedTypes[type] = this.Mapper.MapFor<T>();
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
                     .Where(t => !this.MappedTypes.ContainsKey(t))
                     .AsParallel()
                     .Select(t => this.mapperMapForMethodInfo.MakeGenericMethod(t).Invoke(this.mapper, new object[] { }) as IMap);

            foreach (var map in maps) {
                // force into sequential
                this.MappedTypes[map.Type] = map;
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
        protected Map<T> Setup<T>() {
            this.Dirty();

            IMap map;
            Map<T> mapt;
            var type = typeof(T);

            if (!this.MappedTypes.TryGetValue(type, out map)) {
                this.MappedTypes[type] = mapt = this.Mapper.MapFor<T>(); // just instantiate a Map<T> from scratch
            }
            else {
                mapt = map as Map<T>;

                if (mapt == null) {
                    this.MappedTypes[type] = mapt = Map<T>.From(map); // lift the Map into a Map<T>
                }
            }

            return mapt;
        }
    }
}