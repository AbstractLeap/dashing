namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class InMemoryDatabase {
        public InMemoryDatabase()
            : this(new Dictionary<Type, IList>()) {
        }

        public InMemoryDatabase(IDictionary<Type, IList> testLists) {
        }
    }
}