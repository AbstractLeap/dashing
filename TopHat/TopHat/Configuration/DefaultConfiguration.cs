using System;
using System.Collections.Generic;

namespace TopHat.Configuration {
	public class DefaultConfiguration : ConfigurationBase {
		public DefaultConfiguration(IEngine engine, string connectionString) : base(engine, connectionString) {}

		public new DefaultConfiguration Add<T>() {
			base.Add<T>();
			return this;
		}

		public new DefaultConfiguration Add(IEnumerable<Type> types) {
			base.Add(types);
			return this;
		}

		public new DefaultConfiguration AddNamespaceOf<T>() {
			base.AddNamespaceOf<T>();
			return this;
		}

		public new Map<T> Setup<T>() {
			return base.Setup<T>();
		}
	}
}