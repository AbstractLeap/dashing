namespace Dashing.Tests.Engine.DDL {
    using System;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class DropTableWriterTests {
        private readonly Mock<ISqlDialect> mockDialect = new Mock<ISqlDialect>(MockBehavior.Strict);

        [Fact]
        public void ThrowsOnNullDialect() {
            Assert.Throws<ArgumentNullException>(() => new DropTableWriter(null));
        }

        [Fact]
        public void GeneratesExpectedSql() {
            var target = this.MakeTarget();
            this.mockDialect.Setup(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()))
                .Callback<StringBuilder, IMap>((s, m) => s.Append("<tablename>"));

            var sql = target.DropTable(MakeMap(new Column<string> { Name = "Username" }));

            Assert.Equal("drop table <tablename>", sql);

            this.mockDialect.Verify(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()), Times.Once());
        }

        [Fact]
        public void IfExistsGeneratesExpectedSql() {
            var dialect = new SqlServerDialect();
            var sql = dialect.WriteDropTableIfExists("<tablename>");

            Assert.Equal("if exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '<tablename>') drop table [<tablename>]", sql);
        }

        private static IMap MakeMap(params IColumn[] columns) {
            var cols = new[] { new Column<int> { Name = "DummyId", IsPrimaryKey = true } }.Union(columns).ToArray();
            var map = new Map<User> { Table = "Dummies", PrimaryKey = cols.First() };
            cols.ToList().ForEach(c => map.Columns.Add(c.Name, c));
            return map;
        }

        private DropTableWriter MakeTarget() {
            return new DropTableWriter(this.mockDialect.Object);
        }
    }
}