using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dashing.Configuration;

namespace Dashing.Tools.ReverseEngineering {
    using System.Data;

    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    /// <summary>
    /// This configuration pretty much only exists to support fetching of different maps within reverse engineering
    /// </summary>
    [DoNotWeave]
    internal class Configuration : IReverseEngineeringConfiguration {
        private IDictionary<Type, IMap> maps;

        public Configuration(ISqlDialect sqlDialect) {
            this.maps = new Dictionary<Type, IMap>();
            this.Engine = new SqlEngine(sqlDialect);
        }

        public void AddMap(Type type, IMap map) {
            this.maps.Add(type, map);
        }

        public IEnumerable<IMap> Maps {
            get { return this.maps.Select(k => k.Value); }
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

        public ISession BeginSession() {
            throw new NotImplementedException();
        }

        public ISession BeginSession(System.Data.IDbConnection connection) {
            throw new NotImplementedException();
        }

        public ISession BeginSession(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction) {
            throw new NotImplementedException();
        }

        public ISession BeginTransactionLessSession() {
            throw new NotImplementedException();
        }

        public ISession BeginTransactionLessSession(IDbConnection connection) {
            throw new NotImplementedException();
        }

        public Engine.IEngine Engine { get; set; }

        public IMapper Mapper {
            get { throw new NotImplementedException(); }
        }

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public ICollection<Events.IEventListener> EventListeners {
            get { throw new NotImplementedException(); }
        }

        public Events.EventHandlers EventHandlers {
            get { throw new NotImplementedException(); }
        }
    }
}
