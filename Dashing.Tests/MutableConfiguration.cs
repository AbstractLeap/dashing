namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class MutableConfiguration : BaseConfiguration {
        public new IConfiguration Add<T>() {
            return base.Add<T>();
        }

        public new IConfiguration Add(IEnumerable<Type> types) {
            return base.Add(types);
        }

        public IConfiguration AddNamespaceOf<T>() {
            return base.AddNamespaceOf<T>();
        }

        public new IMap<T> Setup<T>() {
            return base.Setup<T>();
        }
    }
}