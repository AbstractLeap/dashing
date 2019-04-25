namespace Dashing.Tools.Tests.ModelGeneration {
    using Dashing.SchemaReading;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.ModelGeneration;

    using Xunit;

    public class SourceTests {
        private const string TestNamespace = "My.Test";

        [Fact(Skip = "Needs fixing for maps without primary keys")]
        public void NamespaceAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace, null);
            Assert.Contains("namespace " + TestNamespace, results.First().Value);
        }

        [Fact(Skip = "Needs fixing for maps without primary keys")]
        public void PrimaryKeyAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace, null);
            Assert.Contains("public System.Int32 PostId { get; set; }", results["Post"]);
        }

        [Fact(Skip = "Needs fixing for maps without primary keys")]
        public void ParentColumnAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace, null);
            Assert.Contains("public Blog Blog { get; set; }", results["Post"]);
        }

        [Fact(Skip = "Needs fixing for maps without primary keys")]
        public void CollectionColumnAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace, null);
            Assert.Contains("public IList<Comment> Comments { get; set; }", results["Post"]);
        }

        [Fact(Skip = "Needs fixing for maps without primary keys")]
        public void CollectionColumnInitStatementAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace, null);
            Assert.Contains("this.Comments = new List<Comment>();", results["Post"]);
        }

        private Database MakeSchema(CustomConfig config) {
            var tables = new List<TableDto>();
            var columns = new List<ColumnDto>();
            var fks = new List<ForeignKeyDto>();
            foreach (var map in config.Maps) {
                var table = new TableDto();
                table.Name = map.Table;
                foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne)) {
                    columns.Add(new ColumnDto { Name = column.Value.DbName, TableName = column.Value.Map.Table, DbType = column.Value.DbType });
                    fks.Add(
                        new ForeignKeyDto {
                                              ColumnName = column.Value.DbName,
                                              Name = "fk_" + column.Value.DbName,
                                              ReferencedColumnName = column.Value.ParentMap.PrimaryKey.DbName,
                                              ReferencedTableName = column.Value.ParentMap.Table,
                                              TableName = column.Value.Map.Table
                                          });
                }

                tables.Add(table);
            }

            return new Database(tables, columns, null, fks);
        }
    }
}