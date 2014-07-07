namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;

    using Moq;

    public class MockConfiguration : IConfiguration {
        public ConnectionStringSettings ConnectionString { get; set; }

        public Dictionary<Type, IMap> MappedTypes { get; set; } 

        public IGeneratedCodeManager CodeManager { get; set; }

        public IEngine Engine { get; set; }

        public IMapper Mapper { get; set; }

        public bool GetIsTrackedByDefault { get; set; }

        public IEnumerable<IMap> Maps {
            get {
                return this.MappedTypes.Values;
            }
        }

        public Mock<ISession> MockSession { get; set; }
        
        public MockConfiguration() {
            this.ConnectionString = new ConnectionStringSettings { ConnectionString = "Data Source=dummy.local", ProviderName = "System.Data.SqlClient" };
            this.MappedTypes = new Dictionary<Type, IMap>();
            this.MockSession = new Mock<ISession>(MockBehavior.Loose);
            this.Mapper = new DefaultMapper(new DefaultConvention());
        }

        public IMap<T> GetMap<T>() {
            // TODO: check that the Map is indeed an IMap<T> or lift if it isn't
            var map = this.GetMap(typeof(T)) as IMap<T>;
            return map;
        }

        public IMap GetMap(Type type) {
            IMap map;

            if (!this.MappedTypes.TryGetValue(type, out map)) {
                throw new ArgumentException("That type is not mapped");
            }

            return map;
        }

        public bool HasMap(Type type) {
            return this.MappedTypes.ContainsKey(type);
        }

        public ISession BeginSession() {
            return this.MockSession.Object;
        }

        public ISession BeginSession(IDbConnection connection) {
            return this.BeginSession();
        }

        public ISession BeginSession(IDbConnection connection, IDbTransaction transaction) {
            return this.BeginSession();
        }

        public IConfiguration Add<T>() {
            return this.Add(new[] { typeof(T) });
        }

        public IConfiguration Add(IEnumerable<Type> types) {
            foreach (var type in types.Where(t => !this.MappedTypes.ContainsKey(t))) {
                this.MappedTypes[type] = this.Mapper.MapFor(type);
            }

            return this;
        }

        public IConfiguration AddNamespaceOf<T>() {
            var type = typeof(T);
            var ns = type.Namespace;

            if (ns == null) {
                throw new ArgumentException("Namespace of the indicator type is null");
            }

            return this.Add(type.Assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(ns)));
        }

        public IMap<T> Setup<T>() {
            this.Add<T>();
            return this.GetMap<T>();
        }
    }
}