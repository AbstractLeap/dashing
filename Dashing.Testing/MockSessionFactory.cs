namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using Dashing.Configuration;
    using Dashing.Engine;

    public class MockSessionFactory : ISessionFactory {
        private readonly IDictionary<Type, IList> testLists;

        public MockSessionFactory() {
        }

        public MockSessionFactory(IDictionary<Type, IList> testLists) {
            this.testLists = testLists;
        }

        public ISession Create(
            IEngine engine,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool disposeConnection = true,
            bool commitAndDisposeTransaction = false,
            bool isTransactionLess = false) {
            if (this.testLists != null) {
                return new MockSession(this.testLists);
            }
            return new MockSession();
        }
    }
}