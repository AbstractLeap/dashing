using System;
using System.Collections.Generic;
using System.Data;
using TopHat.Configuration;

namespace TopHat {
	public abstract class EngineBase : IEngine {
		protected IDictionary<Type, Map> Maps { get; set; }

		public abstract IDbConnection Open(string connectionString);

		public abstract IEnumerable<T> Query<T>(IDbConnection connection, ISelect<T> query);

		public void UseMaps(IDictionary<Type, Map> maps) {
			Maps = maps;
		}
	}
}