using System;
using System.Collections.Generic;
using System.Data;
using TopHat.Configuration;

namespace TopHat {
	public interface IEngine {
		IDbConnection Open(string connectionString);

		void UseMaps(IDictionary<Type, Map> maps);

		IEnumerable<T> Query<T>(IDbConnection connection, ISelect<T> query);
	}
}