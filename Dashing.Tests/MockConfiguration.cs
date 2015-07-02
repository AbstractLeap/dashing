namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Events;

    using Moq;

    public class MockConfiguration : IConfiguration {
        public IMapper Mapper { get; private set; }

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public ISession BeginTransactionLessSession(IDbConnection connection) {
            throw new NotImplementedException();
        }

        public IEngine Engine {
            get {
                return this.MockEngine.Object;
            }
        }

        public IEnumerable<IMap> Maps {
            get {
                return this.mappedTypes.Values;
            }
        }

        public Mock<ISession> MockSession { get; set; }

        public Mock<IEngine> MockEngine { get; set; }

        private readonly Dictionary<Type, IMap> mappedTypes;

        public MockConfiguration() {
            this.MockSession = new Mock<ISession>(MockBehavior.Loose);
            this.MockEngine = new Mock<IEngine>(MockBehavior.Loose);
            this.Mapper = new DefaultMapper(new DefaultConvention());
            this.mappedTypes = new Dictionary<Type, IMap>();
        }

        public IMap<T> GetMap<T>() {
            // TODO: check that the Map is indeed an IMap<T> or lift if it isn't
            var map = this.GetMap(typeof(T)) as IMap<T>;
            return map;
        }

        public IMap GetMap(Type type) {
            return ConfigurationHelper.GetMap(type, this.mappedTypes);
        }

        public bool HasMap(Type type) {
            return ConfigurationHelper.HasMap(type, this.mappedTypes);
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

        public ISession BeginTransactionLessSession() {
            throw new NotImplementedException();
        }

        public MockConfiguration Add<T>() {
            ConfigurationHelper.Add<T>(this, this.mappedTypes);
            return this;
        }

        public MockConfiguration Add(IEnumerable<Type> types) {
            ConfigurationHelper.Add(this, this.mappedTypes, types);
            return this;
        }

        public MockConfiguration AddNamespaceOf<T>() {
            ConfigurationHelper.AddNamespaceOf<T>(this, this.mappedTypes);
            return this;
        }

        public IMap<T> Setup<T>() {
            return ConfigurationHelper.Setup<T>(this, this.mappedTypes);
        }

        public ICollection<IEventListener> EventListeners {
            get {
                throw new NotImplementedException();
            }
        }

        public EventHandlers EventHandlers {
            get {
                throw new NotImplementedException();
            }
        }
    }
}