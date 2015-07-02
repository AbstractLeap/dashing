namespace Dashing.Testing {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Events;

    public class MockConfiguration : IConfiguration {

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public IMap<T> GetMap<T>() {
            throw new NotImplementedException();
        }

        public IMap GetMap(Type type) {
            throw new NotImplementedException();
        }

        public bool HasMap(Type type) {
            throw new NotImplementedException();
        }

        public ISession BeginSession() {
            throw new NotImplementedException();
        }

        public ISession BeginSession(IDbConnection connection) {
            throw new NotImplementedException();
        }

        public ISession BeginSession(IDbConnection connection, IDbTransaction transaction) {
            throw new NotImplementedException();
        }

        public ISession BeginTransactionLessSession() {
            throw new NotImplementedException();
        }

        public ISession BeginTransactionLessSession(IDbConnection connection) {
            throw new NotImplementedException();
        }

        public IEnumerable<IMap> Maps {
            get {
                throw new NotImplementedException();
            }
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

        public IEngine Engine {
            get {
                throw new NotImplementedException();
            }
        }

        public IMapper Mapper {
            get {
                throw new NotImplementedException();
            }
        }
    }
}