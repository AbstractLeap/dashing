namespace Dashing.IntegrationTests.Tests.dash {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    using Dapper;

    using Dashing.Cli;
    using Dashing.IntegrationTests.TestDomain.Versioned;
    using Dashing.IntegrationTests.TestDomain.Versioned.NonVersioned;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class VersionedEntityTests : IDisposable {
        private readonly ITestOutputHelper output;

        private static string connectionString = "Server=localhost;Database=versionedentitytests;Trusted_Connection=True;MultipleActiveResultSets=True";

        public VersionedEntityTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void GeneratedVersionedEntityDoesntNeedUpdate() {
            var databaseMigrator = new DatabaseMigrator();
            var configuration = new VersionedConfiguration();
            var answerProvider = new Mock<IAnswerProvider>();
            databaseMigrator.Execute(configuration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);

            var scriptGenerator = new ScriptGenerator();
            var script = scriptGenerator.Generate(configuration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);
            this.output.WriteLine(script);
            Assert.True(string.IsNullOrWhiteSpace(script));
        }

        [Fact]
        public void AddingSystemVersioningWorks() {
            var databaseMigrator = new DatabaseMigrator();
            var configuration = new NonVersionedConfiguration();
            var answerProvider = new Mock<IAnswerProvider>();
            databaseMigrator.Execute(configuration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);

            var versionedConfiguration = new VersionedConfiguration();
            var scriptGenerator = new ScriptGenerator();
            var migrateScript = scriptGenerator.Generate(versionedConfiguration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);
            this.output.WriteLine(migrateScript);

            var databaseMigrator2 = new DatabaseMigrator();
            databaseMigrator2.Execute(versionedConfiguration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);

            var scriptGenerator2 = new ScriptGenerator();
            var script = scriptGenerator2.Generate(versionedConfiguration, connectionString, "System.Data.SqlClient", Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<KeyValuePair<string, string>>(), false, answerProvider.Object);
            this.output.WriteLine(script);
            Assert.True(string.IsNullOrWhiteSpace(script));
        }

        public void Dispose() {
            using (var connection = new SqlConnection(connectionString)) {
                connection.Open();
                connection.Execute(
                    @"
ALTER TABLE [dbo].[VersionedEntities] SET ( SYSTEM_VERSIONING = OFF  );
DROP TABLE [dbo].[VersionedEntities];
DROP TABLE [dbo].[VersionedEntitiesHistory];
");
            }
        }
    }
}