using System;
using System.Collections.Generic;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat {
	public abstract class EngineBase : IEngine {
		private readonly IConnectionFactory _connectionFactory;
		private readonly ISqlWriter _sqlWriter;

		protected EngineBase(IConnectionFactory connectionFactory, ISqlWriter sqlWriter) {
			_connectionFactory = connectionFactory;
			_sqlWriter = sqlWriter;
		}

		public IConnectionFactory ConnectionFactory {
			get { return _connectionFactory; }
		}

		public ISqlWriter SqlWriter {
			get { return _sqlWriter; }
		}

		public void UseMaps(IDictionary<Type, Map> maps) {
			if (SqlWriter == null) throw new InvalidOperationException("SqlWriter is null");
			SqlWriter.UseMaps(maps);
		}
	}
}