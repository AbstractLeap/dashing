using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TopHat.Configuration {
	// Dear ReSharper, we are doing this deliberately so that the lovely users can extend
	// ReSharper disable MemberCanBePrivate.Global
	public abstract class ConfigurationBase : IConfiguration {
		private IEngine _engine;

		private readonly String _connectionString;

		protected IMapper Mapper { get; set; }

		protected ISessionFactory SessionFactory { get; set; }

		protected IQueryFactory QueryFactory { get; set; }

		protected IDictionary<Type, Map> MappedTypes { get; set; }

		protected Boolean EngineHasLatestMaps { get; set; }

		public IEnumerable<Map> Maps {
			get { return MappedTypes.Values; }
		}

		protected IEngine Engine {
			get {
				if (!EngineHasLatestMaps) {
					Engine.UseMaps(MappedTypes);
					EngineHasLatestMaps = true;
				}
				return _engine;
			}
			set {
				Dirty();
				_engine = value;
			}
		}

		protected ConfigurationBase(IEngine engine, String connectionString, IMapper mapper, ISessionFactory sessionFactory, IQueryFactory queryFactory) {
			if (engine == null) throw new ArgumentNullException("engine");
			if (connectionString == null) throw new ArgumentNullException("connectionString");
			if (mapper == null) throw new ArgumentNullException("mapper");
			if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
			if (queryFactory == null) throw new ArgumentNullException("queryFactory");

			_engine = engine;
			_connectionString = connectionString;
			Mapper = mapper;
			SessionFactory = sessionFactory;
			QueryFactory = queryFactory;
			MappedTypes = new Dictionary<Type, Map>();
		}

		// Poor man's DI
		public ConfigurationBase(IEngine engine, string connectionString)
			: this(engine, connectionString, new DefaultMapper(new DefaultConvention()), new DefaultSessionFactory(), new DefaultQueryFactory()) {}

		public ISession BeginSession() {
			return SessionFactory.Create(
				Engine,
				QueryFactory,
				Engine.Open(_connectionString));
		}

		public ISession BeginSession(IDbConnection connection) {
			return SessionFactory.Create(
				Engine,
				QueryFactory,
				connection);
		}

		public ISession BeginSession(IDbConnection connection, IDbTransaction transaction) {
			return SessionFactory.Create(
				Engine,
				QueryFactory,
				connection,
				transaction);
		}

		protected Map<T> Setup<T>() {
			Dirty();

			Map map;
			Map<T> mapt;
			var type = typeof (T);

			if (!MappedTypes.TryGetValue(type, out map))
				MappedTypes[type] = mapt = Mapper.MapFor<T>(); // just instantiate a Map<T> from scratch
			else {
				mapt = map as Map<T>;

				if (mapt == null)
					MappedTypes[type] = mapt = Map<T>.From(map); // lift the Map into a Map<T>
			}

			return mapt;
		}

		protected IConfiguration Add<T>() {
			Dirty();

			var type = typeof (T);
			if (!MappedTypes.ContainsKey(type))
				MappedTypes[type] = Mapper.MapFor(type);
			return this;
		}

		protected IConfiguration Add(IEnumerable<Type> types) {
			Dirty();

			types.Distinct().Where(t => !MappedTypes.ContainsKey(t)).AsParallel().ForAll(t => MappedTypes[t] = Mapper.MapFor(t));
			return this;
		}

		protected IConfiguration AddNamespaceOf<T>() {
			Dirty();

			var type = typeof (T);
			var ns = type.Namespace;
			if (ns == null) throw new ArgumentException("Namespace of the indicator type is null");
			return Add(type.Assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(ns)));
		}

		private void Dirty() {
			EngineHasLatestMaps = false;
		}
	}
}