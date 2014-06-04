namespace TopHat.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using TopHat.Engine;

    public class MutableConfiguration : DefaultConfiguration {
        public MutableConfiguration(IEngine engine, string connectionString) : base(engine, connectionString) { }

        public IConfiguration Add<T>()
        {
            return base.Add<T>();
        }

        public IConfiguration Add(IEnumerable<Type> types) {
            return base.Add(types);
        }

        public IConfiguration AddNamespaceOf<T>() {
            return base.AddNamespaceOf<T>();
        }

        public Map<T> Setup<T>() {
            return base.Setup<T>();
        }
    }
}