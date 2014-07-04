namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Dashing.Engine;

    public class MutableConfiguration : DefaultConfiguration {
        public MutableConfiguration(IEngine engine, string connectionString)
            : base(engine, connectionString) { }

        public MutableConfiguration(ConnectionStringSettings connectionString)
            : base(connectionString)
        {
        }

        public new IConfiguration Add<T>() {
            return base.Add<T>();
        }

        public new IConfiguration Add(IEnumerable<Type> types) {
            return base.Add(types);
        }

        public new IConfiguration AddNamespaceOf<T>() {
            return base.AddNamespaceOf<T>();
        }

        public new IMap<T> Setup<T>() {
            return base.Setup<T>();
        }
    }
}