using System;
using System.Collections.Generic;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat.SqlServer {
	public class SqlServerSqlWriter : ISqlWriter {
		private IDictionary<Type, Map> _maps;

		public void UseMaps(IDictionary<Type, Map> maps) {
			if (maps == null) throw new ArgumentNullException("maps");

			_maps = maps;
		}

		public SqlWriterResult WriteSqlFor<T>(Query<T> query) {
			var type = typeof (T);
			if (!_maps.ContainsKey(type))
				throw new Exception("Entity is not mapped");

			throw new NotImplementedException();
		}
	}
}