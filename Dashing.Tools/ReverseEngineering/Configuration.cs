using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dashing.Configuration;

namespace Dashing.Tools.ReverseEngineering {
    /// <summary>
    /// This configuration pretty much only exists to support fetching of different maps within reverse engineering
    /// </summary>
    internal class Configuration : IReverseEngineeringConfiguration {
        private IDictionary<Type, IMap> maps;

        public Configuration() {
            this.maps = new Dictionary<Type, IMap>();
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

        public CodeGeneration.IGeneratedCodeManager CodeManager {
            get { throw new NotImplementedException(); }
        }

        public Engine.IEngine Engine {
            get { throw new NotImplementedException(); }
        }

        public IMapper Mapper {
            get { throw new NotImplementedException(); }
        }

        public bool GetIsTrackedByDefault {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
