namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.TestDomain;

    using Xunit;

    public class AlterTests {
        [Fact]
        public void DropOneToOneLeftTable() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();
            
            // remove onetooneleft from the config
            var mappedTypes = (IDictionary<Type, IMap>)typeof(ConfigurationBase).GetField("mappedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(OneToOneLeft));
            configTo.GetMap<OneToOneRight>().Columns.Remove("Left");

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), new AlterTableWriter(dialect));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(configFrom.Maps, configTo.Maps, null, null, new string[0], out warnings, out errors);

            var dropColIdx = script.IndexOf("alter table [OneToOneRights] drop column [LeftId];", System.StringComparison.Ordinal);
            var dropTableIdx = script.IndexOf("drop table [OneToOneLefts];", System.StringComparison.Ordinal);
            Assert.True(dropColIdx > -1);
            Assert.True(dropTableIdx > -1);
            Assert.True(dropColIdx < dropTableIdx);
        }

        [Fact]
        public void DropOneToOneRightTable() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();

            // remove onetooneleft from the config
            var mappedTypes = (IDictionary<Type, IMap>)typeof(ConfigurationBase).GetField("mappedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(OneToOneRight));
            configTo.GetMap<OneToOneLeft>().Columns.Remove("Right");

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), new AlterTableWriter(dialect));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(configFrom.Maps, configTo.Maps, null, null, new string[0], out warnings, out errors);

            var dropColIdx = script.IndexOf("alter table [OneToOneLefts] drop column [RightId];", System.StringComparison.Ordinal);
            var dropTableIdx = script.IndexOf("drop table [OneToOneRights];", System.StringComparison.Ordinal);
            Assert.True(dropColIdx > -1);
            Assert.True(dropTableIdx > -1);
            Assert.True(dropColIdx < dropTableIdx);
        }

        [Fact]
        public void DropSelfReferencedWorks() {
            var configTo = new CustomConfig();
            var configFrom = new CustomConfig();

            // remove onetooneleft from the config
            var mappedTypes = (IDictionary<Type, IMap>)typeof(ConfigurationBase).GetField("mappedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(configTo);
            mappedTypes.Remove(typeof(Pair));

            var dialect = new SqlServer2012Dialect();
            var migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), new AlterTableWriter(dialect));
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(configFrom.Maps, configTo.Maps, null, null, new string[0], out warnings, out errors);

            Assert.Equal(@"if exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Pairs') drop table [Pairs];
", script);
        }
    }
}