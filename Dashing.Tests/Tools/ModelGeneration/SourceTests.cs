namespace Dashing.Tests.Tools.ModelGeneration {
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Tests.TestDomain;
    using Dashing.Tools.ModelGeneration;

    using DatabaseSchemaReader.DataSchema;

    using Moq;

    using Xunit;

    public class SourceTests {
        private const string TestNamespace = "My.Test";

        [Fact]
        public void NamespaceAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace);
            Assert.Contains("namespace " + TestNamespace, results.First().Value);
        }

        [Fact]
        public void PrimaryKeyAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace);
            Assert.Contains("public System.Int32 PostId { get; set; }", results["Post"]);
        }

        [Fact]
        public void ParentColumnAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace);
            Assert.Contains("public Blog Blog { get; set; }", results["Post"]);
        }

        [Fact]
        public void CollectionColumnAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace);
            Assert.Contains("public IList<Comment> Comments { get; set; }", results["Post"]);
        }

        [Fact]
        public void CollectionColumnInitStatementAdded() {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, this.MakeSchema(config), TestNamespace);
            Assert.Contains("this.Comments = new List<Comment>();", results["Post"]);
        }

        private DatabaseSchema MakeSchema(CustomConfig config) {
            var result = new DatabaseSchema(string.Empty, SqlType.SqlServer);
            foreach (var map in config.Maps) {
                var table = new DatabaseTable();
                table.Name = map.Table;
                foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne)) {
                    table.Columns.Add(new DatabaseColumn { ForeignKeyTableName = config.GetMap(column.Value.Type).Table, Name = column.Value.DbName });
                }

                result.Tables.Add(table);
            }

            return result;
        }
    }

    public class CustomConfig : MockConfiguration {
        public CustomConfig() {
            this.AddNamespaceOf<Post>();
        }
    }
}