namespace Dashing.ReverseEngineering {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Events;

    using Poly.Logging;

    /// <summary>
    ///     This configuration pretty much only exists to support fetching of different maps within reverse engineering
    /// </summary>
    internal class Configuration : IReverseEngineeringConfiguration {
        private readonly IDictionary<Type, IMap> maps;

        public Configuration(ISqlDialect sqlDialect) {
            this.maps = new Dictionary<Type, IMap>();
            this.Engine = new SqlEngine(this, sqlDialect);
        }

        public IPolyLogger Logger { get; }

        public void AddMap(Type type, IMap map) {
            this.maps.Add(type, map);
        }

        public IEnumerable<IMap> Maps {
            get {
                return this.maps.Select(k => k.Value);
            }
        }

        public IMap<T> GetMap<T>() {
            return Map<T>.From(this.GetMap(typeof(T)));
        }

        public IMap GetMap(Type type) {
            if (!this.HasMap(type)) {
                throw new ArgumentOutOfRangeException("type", "The type " + type.Name + " is not mapped");
            }

            return this.maps[type];
        }

        public bool HasMap(Type type) {
            return this.maps.ContainsKey(type);
        }

        public bool HasMap<T>() {
            return this.HasMap(typeof(T));
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

        public IEngine Engine { get; set; }

        public IMapper Mapper {
            get {
                throw new NotImplementedException();
            }
        }

        public bool CompleteFailsSilentlyIfRejected { get; set; }

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