namespace TopHat.Tests.Engine.DDL {
    using System;
    using System.Linq;
    using System.Text;

    using Moq;

    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Engine.DDL;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class CreateTableWriterTests {
        private readonly Mock<ISqlDialect> mockDialect = new Mock<ISqlDialect>(MockBehavior.Strict);

        [Fact]
        public void ThrowsOnNullDialect() {
            Assert.Throws<ArgumentNullException>(() => new CreateTableWriter(null));
        }

        [Fact]
        public void SimpleMapGeneratesExpectedSql() {
            var target = this.MakeTarget();

            this.mockDialect.Setup(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>())).Callback<StringBuilder, IMap>((s, m) => s.Append("<tablename>"));

            this.mockDialect.Setup(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()))
                .Callback<StringBuilder, IColumn>((s, m) => s.Append("<colspec:" + m.Name + ">"));

            var sql = target.CreateTable(MakeMap(new Column<string> { Name = "Username" }));

            Assert.Equal("create table <tablename> (<colspec:DummyId>, <colspec:Username>)", sql);

            this.mockDialect.Verify(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()), Times.Once());
            this.mockDialect.Verify(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()), Times.Exactly(2));
        }

        private static IMap MakeMap(params IColumn[] columns) {
            var cols = new[] { new Column<int> { Name = "DummyId", IsPrimaryKey = true } }.Union(columns).ToArray();
            return new Map<User> { Table = "Dummies", Columns = cols.ToDictionary(c => c.Name, c => c), PrimaryKey = cols.First() };
        }

        [Fact]
        public void IgnoredPropertyExcludedFromColumnSpecification() {
            this.mockDialect.Setup(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()));
            this.mockDialect.Setup(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()));

            var target = this.MakeTarget();
            target.CreateTable(MakeMap(new Column<string> { Name = "Ignored", IsIgnored = true }));

            this.mockDialect.Verify(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()), Times.Once());
        }

        [Fact]
        public void OneToManyPropertyExcludedFromColumnSpecification() {
            this.mockDialect.Setup(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()));
            this.mockDialect.Setup(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()));

            var target = this.MakeTarget();
            target.CreateTable(MakeMap(new Column<string> { Name = "Ignored", Relationship = RelationshipType.ManyToOne }));

            this.mockDialect.Verify(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()), Times.Once());
        }

        [Fact]
        public void ManyToManyPropertyExcludedFromColumnSpecification() {
            this.mockDialect.Setup(m => m.AppendQuotedTableName(It.IsAny<StringBuilder>(), It.IsAny<IMap>()));
            this.mockDialect.Setup(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()));

            var target = this.MakeTarget();
            target.CreateTable(MakeMap(new Column<string> { Name = "Ignored", Relationship = RelationshipType.ManyToMany }));

            this.mockDialect.Verify(m => m.AppendColumnSpecification(It.IsAny<StringBuilder>(), It.IsAny<IColumn>()), Times.Once());
        }

        private CreateTableWriter MakeTarget() {
            return new CreateTableWriter(this.mockDialect.Object);
        }
    }
}