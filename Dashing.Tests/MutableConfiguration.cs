namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Dashing.Configuration;

    public class MutableConfiguration : DefaultConfiguration {
        public MutableConfiguration(ConnectionStringSettings connectionStringSettings)
            : base(connectionStringSettings) {
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