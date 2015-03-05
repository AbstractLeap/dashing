namespace Dashing.Tools.Tests.Migration {
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.TestDomain;

    using Xunit;

    public class IndexTests {
        [Fact]
        public void IgnoredIndexNotDeleted() {
            var migrator = MakeMigrator();
            var config = new SimpleClassConfig();
            var config2 = new SimpleClassConfig();
            config.Maps.ElementAt(0).AddIndex(new Index(config.Maps.ElementAt(0), new[] { config.Maps.ElementAt(0).Columns.First().Value }, "Foo", false));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(config.Maps, config2.Maps, null, null, new[] { "Foo" }, out warnings, out errors);
            Assert.Empty(script);
        }

        [Fact]
        public void NonIgnoredIndexDropped() {
            var migrator = MakeMigrator();
            var config = new SimpleClassConfig();
            var config2 = new SimpleClassConfig();
            config.Maps.ElementAt(0).AddIndex(new Index(config.Maps.ElementAt(0), new[] { config.Maps.ElementAt(0).Columns.First().Value }, "Foo", false));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(config.Maps, config2.Maps, null, null, new string[0], out warnings, out errors);
            Assert.Equal(@"drop index [Foo] on [SimpleClasses];
", script);
        }

        private static Migrator MakeMigrator() {
            var migrator = new Migrator(
                new CreateTableWriter(new SqlServerDialect()),
                new DropTableWriter(new SqlServerDialect()),
                new AlterTableWriter(new SqlServerDialect()));
            return migrator;
        }

        private static ConnectionStringSettings ConnectionString {
            get {
                return new ConnectionStringSettings("DefaultDb", string.Empty, "System.Data.SqlClient");
            }
        }

        private class SimpleClassConfig : DefaultConfiguration {
            public SimpleClassConfig()
                : base(ConnectionString) {
                this.Add<SimpleClass>();
            }
        }
    }
}