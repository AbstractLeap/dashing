using System;
using System.Collections.Generic;
using TopHat.Configuration;

namespace TopHat.SqlWriter {
	public interface ISqlWriter {
		void UseMaps(IDictionary<Type, Map> maps);

		SqlWriterResult WriteSqlFor<T>(Query<T> query);
	}
}