namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Events;
    using Dashing.Extensions;
#if COREFX
    using System.Reflection;
#endif

    public abstract class BaseConfiguration : IConfiguration {
        private readonly IMapper mapper;

        private IDictionary<Type, IMap> mappedTypes;

        private EventHandlers eventHandlers;

        public IEnumerable<IMap> Maps {
            get {
                return this.mappedTypes.Values;
            }
        }

        public BaseConfiguration() : this(new DefaultMapper(new DefaultConvention())) {

        }

        public BaseConfiguration(IMapper mapper) {
            this.mapper = mapper;
            this.mappedTypes = new Dictionary<Type, IMap>();
            this.eventHandlers = new EventHandlers(new List<IEventListener>());
        }

        public IMap<T> GetMap<T>() {
            var map = this.GetMap(typeof(T)) as IMap<T>;
            return map;
        }

        public IMap GetMap(Type type) {
            IMap map;

            // shortcut for simplest case
            if (this.mappedTypes.TryGetValue(type, out map)) {
                return map;
            }

            // definitely not mapped
            throw new ArgumentException(string.Format("Type {0} is not mapped", type.Name));
        }

        public bool HasMap<T>() {
            return this.HasMap(typeof(T));
        }

        public bool HasMap(Type type) {
            return this.mappedTypes.ContainsKey(type);
        }

        public EventHandlers EventHandlers { get; }

        protected virtual IConfiguration Add<T>() {
            this.Add(new[] { typeof(T) });
            return this;
        }

        protected virtual IConfiguration Add(IEnumerable<Type> types) {
            var maps = types.Distinct().Where(t => !this.mappedTypes.ContainsKey(t))
                .Select(t => this.mapper.MapFor(t, this));
            foreach (var map in maps) {
                this.mappedTypes.Add(map.Type, map);
            }

            return this;
        }

        protected virtual IConfiguration AddNamespaceOf<T>() {
            var type = typeof(T);
            var ns = type.Namespace;

            if (ns == null) {
                throw new ArgumentException("Namespace of the indicator type is null");
            }

            this.Add(type.Assembly().GetTypes().Where(t => t.IsClass() && !t.IsAbstract() && t.Namespace != null && t.Namespace == ns && !typeof(IConfiguration).IsAssignableFrom(t)));
            return this;
        }

        protected virtual IMap<T> Setup<T>() {
            this.Add<T>();
            return this.GetMap<T>();
        }

        protected virtual IConfiguration AddEventListener(IEventListener eventListener) {
            if (eventListener is IOnPreInsertEventListener) {
                this.eventHandlers.PreInsertListeners.Add(eventListener as IOnPreInsertEventListener);
            }

            if (eventListener is IOnPreSaveEventListener) {
                this.eventHandlers.PreSaveListeners.Add(eventListener as IOnPreSaveEventListener);
            }

            if (eventListener is IOnPreDeleteEventListener) {
                this.eventHandlers.PreDeleteListeners.Add(eventListener as IOnPreDeleteEventListener);
            }

            if (eventListener is IOnPostInsertEventListener) {
                this.eventHandlers.PostInsertListeners.Add(eventListener as IOnPostInsertEventListener);
            }

            if (eventListener is IOnPostSaveEventListener) {
                this.eventHandlers.PostSaveListeners.Add(eventListener as IOnPostSaveEventListener);
            }

            if (eventListener is IOnPostDeleteEventListener) {
                this.eventHandlers.PostDeleteListeners.Add(eventListener as IOnPostDeleteEventListener);
            }

            return this;
        }
    }

    //public class DashingInMemory : IDashing {
    //    public DashingInMemory(IConfiguration configuration) {

    //    }

    //    public ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null) {
    //        throw new NotImplementedException();
    //    }

    //    public ISession BeginTransactionLessSession(IDbConnection connection = null) {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class ExampleUsage {
    //    public class MyConfig : BaseConfiguration {
    //        public MyConfig() {
    //            this.Add<Foo>();
    //        }
    //    }

    //    public class CommandThatDoesStuff {
    //        public async Task Execute() {
    //            var config = new MyConfig();
    //            var dashing = new DashingSql(config, DbProviderFactories.GetFactory("System.Data.SqlClient"), "connectionstring");
    //            using (var session = dashing.BeginSession()) {
    //                var tings = await session.Query<Foo>().ToListAsync();
    //                session.Complete();
    //            }
    //        }
    //    }

    //    public class Foo {

    //    }
    //}
}