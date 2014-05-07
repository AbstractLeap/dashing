using System;
using System.Collections.Generic;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat {
	public interface IEngine {
		IConnectionFactory ConnectionFactory { get; }
		ISqlWriter SqlWriter { get; }

		void UseMaps(IDictionary<Type, Map> maps);
	}
}