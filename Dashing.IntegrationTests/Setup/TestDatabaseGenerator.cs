namespace Dashing.IntegrationTests.Setup {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public class TestDatabaseGenerator {
        private static readonly IList<TestSessionWrapper> TestSessions = new List<TestSessionWrapper>(); 

        static TestDatabaseGenerator() {
            // delete the sqlite db if exists
            if (File.Exists("testdb.db")) {
                File.Delete("testdb.db");
            }

            // generate all the sessions
            var sessionCreatorTypes = typeof(TestDatabaseGenerator).Assembly().GetTypes().Where(t => t.Namespace == "Dashing.IntegrationTests.Setup" && typeof(IDatabase).IsAssignableFrom(t) && t.IsPublic()).ToArray();
            var config = new Configuration();
            foreach (var sessionCreatorType in sessionCreatorTypes) {
                var sessionCreator = (SqlDatabase)Activator.CreateInstance(sessionCreatorType, config);
                var dbInitializer = new DatabaseInitializer(sessionCreator, config);
                TestSessions.Add(dbInitializer.Initialize());
            }
        }

        public static IEnumerable<TestSessionWrapper> SessionWrappers {
            get {
                return TestSessions;
            }
        }
    }
}