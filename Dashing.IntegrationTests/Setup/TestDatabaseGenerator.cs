namespace Dashing.IntegrationTests.Setup {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;

    public class TestDatabaseGenerator {
        private static readonly IList<TestSessionWrapper> TestSessions = new List<TestSessionWrapper>(); 

        static TestDatabaseGenerator() {
            // delete the sqlite db if exists
            if (File.Exists("testdb.db")) {
                File.Delete("testdb.db");
            }

            // generate all the sessions
            var configurationTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Dashing.IntegrationTests.Setup" && typeof(IConfiguration).IsAssignableFrom(t)).ToArray();
            foreach (var configurationType in configurationTypes) {
                var configuration = (IConfiguration)Activator.CreateInstance(configurationType);
                var dbInitializer = new DatabaseInitializer(configuration);
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