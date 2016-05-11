namespace Dashing.IntegrationTests.Setup {
    using System.Collections.Generic;
    using System.Linq;

    public class SessionDataGenerator {
        public static IEnumerable<object[]> GetSessions() {
            return TestDatabaseGenerator.SessionWrappers.Select(wrapper => new object[] { wrapper });
        }  
    }
}