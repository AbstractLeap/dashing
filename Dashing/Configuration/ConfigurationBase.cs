namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Engine;
    using Dashing.Events;

    public abstract class ConfigurationBase : IConfiguration {
        private readonly ConnectionStringSettings connectionStringSettings;

        private readonly IMapper mapper;

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

        public IEngine Engine { get; private set; }

        public IGeneratedCodeManager CodeManager {
            get {
                return this.codeManager ?? (this.codeManager = this.codeGenerator.Generate(this));
            }
        }

        public ICollection<IEventListener> EventListeners {
            get;
            private set;
        }

        public EventHandlers EventHandlers { get; private set; }

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
            this.mappedTypes = new Dictionary<Type, IMap>();
            
            var eventListeners = new ObservableCollection<IEventListener>();
            eventListeners.CollectionChanged += this.EventListenersCollectionChanged;
            this.EventListeners = eventListeners;
            this.EventHandlers = new EventHandlers(this.EventListeners);
        }

        private void EventListenersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // let's make this real simple, just invalidate the eventhandlers property
            this.EventHandlers.Invalidate(this.EventListeners);
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
            return this.sessionFactory.Create(this.Engine, connection, transaction: transaction, disposeConnection: false);
        }

        public ISession BeginTransactionLessSession() {
            var connection = this.dbProviderFactory.CreateConnection();
            if (connection == null) {
                throw new InvalidOperationException("Could not create a connection using the supplied DbProviderFactory");
            }

            connection.ConnectionString = this.connectionStringSettings.ConnectionString;
            return this.sessionFactory.Create(this.Engine, connection, isTransactionLess: true);
        }

        public ISession BeginTransactionLessSession(IDbConnection connection) {
            return this.sessionFactory.Create(this.Engine, connection, isTransactionLess: true);
        }

        protected IConfiguration Add<T>() {
            this.Dirty();
            ConfigurationHelper.Add<T>(this, this.mappedTypes);
            return this;
        }

        protected IConfiguration Add(IEnumerable<Type> types) {
            this.Dirty();
            ConfigurationHelper.Add(this, this.mappedTypes, types);
            return this;
        }

        protected IConfiguration AddNamespaceOf<T>() {
            this.Dirty();
            ConfigurationHelper.AddNamespaceOf<T>(this, this.mappedTypes);
            return this;
        }

        protected IMap<T> Setup<T>() {
            this.Dirty();
            return ConfigurationHelper.Setup<T>(this, this.mappedTypes);
        }
    }
}