namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class AlterTests {
        [Fact]
        public void DropOneToOneLeftTable() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();

            // remove onetooneleft from the config
            var mappedTypes =
                (IDictionary<Type, IMap>)
                typeof(ConfigurationBase).GetField("mappedTypes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(OneToOneLeft));
            configTo.GetMap<OneToOneRight>().Columns.Remove("Left");

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(
                dialect,
                new CreateTableWriter(dialect),
                new AlterTableWriter(dialect),
                new DropTableWriter(dialect),
                GetMockStatisticsProvider(configFrom));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                configFrom.Maps,
                configTo.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                out warnings,
                out errors);

            var dropColIdx = script.IndexOf("alter table [OneToOneRights] drop column [LeftId];", StringComparison.Ordinal);
            var dropTableIdx = script.IndexOf("drop table [OneToOneLefts];", StringComparison.Ordinal);
            Assert.True(dropColIdx > -1);
            Assert.True(dropTableIdx > -1);
            Assert.True(dropColIdx < dropTableIdx);
        }

        [Fact]
        public void DropOneToOneRightTable() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();

            // remove onetooneleft from the config
            var mappedTypes =
                (IDictionary<Type, IMap>)
                typeof(ConfigurationBase).GetField("mappedTypes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(OneToOneRight));
            configTo.GetMap<OneToOneLeft>().Columns.Remove("Right");

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(
                dialect,
                new CreateTableWriter(dialect),
                new AlterTableWriter(dialect),
                new DropTableWriter(dialect),
                GetMockStatisticsProvider(configFrom));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                configFrom.Maps,
                configTo.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                out warnings,
                out errors);

            var dropColIdx = script.IndexOf("alter table [OneToOneLefts] drop column [RightId];", StringComparison.Ordinal);
            var dropTableIdx = script.IndexOf("drop table [OneToOneRights];", StringComparison.Ordinal);
            Assert.True(dropColIdx > -1);
            Assert.True(dropTableIdx > -1);
            Assert.True(dropColIdx < dropTableIdx);
        }

        [Fact]
        public void DropSelfReferencedWorks() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();

            // remove onetooneleft from the config
            var mappedTypes =
                (IDictionary<Type, IMap>)
                typeof(ConfigurationBase).GetField("mappedTypes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(Pair));

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(
                dialect,
                new CreateTableWriter(dialect),
                new AlterTableWriter(dialect),
                new DropTableWriter(dialect),
                GetMockStatisticsProvider(configFrom));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                configFrom.Maps,
                configTo.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                out warnings,
                out errors);

            Assert.Equal(@"drop table [Pairs];", script.Trim());
        }

        private IStatisticsProvider GetMockStatisticsProvider(IConfiguration config) {
            var mock = new Mock<IStatisticsProvider>();
            mock.Setup(m => m.GetStatistics(It.IsAny<IEnumerable<IMap>>())).Returns(config.Maps.ToDictionary(k => k.Type.Name, k => new Statistics()));
            return mock.Object;
        }
    }
}