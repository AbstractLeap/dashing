using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TopHat.Tools.ModelGeneration;
using TopHat.Configuration;

namespace TopHat.Tests.Tools.ModelGeneration
{
    public class SourceTests
    {
        const string testNamespace = "My.Test";

        [Fact]
        public void NamespaceAdded()
        {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, MakeSchema(config), testNamespace);
            Assert.Contains("namespace " + testNamespace, results.First().Value);
        }

        [Fact]
        public void PrimaryKeyAdded()
        {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, MakeSchema(config), testNamespace);
            Assert.Contains("public System.Int32 PostId { get; set; }", results["Post"]);
        }

        [Fact]
        public void ParentColumnAdded()
        {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, MakeSchema(config), testNamespace);
            Assert.Contains("public Blog Blog { get; set; }", results["Post"]);
        }

        [Fact]
        public void CollectionColumnAdded()
        {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, MakeSchema(config), testNamespace);
            Assert.Contains("public IList<Comment> Comments { get; set; }", results["Post"]);
        }

        [Fact]
        public void CollectionColumnInitStatementAdded()
        {
            var generator = new ModelGenerator();
            var config = new CustomConfig();
            var results = generator.GenerateFiles(config.Maps, MakeSchema(config), testNamespace);
            Assert.Contains("this.Comments = new List<Comment>();", results["Post"]);
        }

        private DatabaseSchemaReader.DataSchema.DatabaseSchema MakeSchema(CustomConfig config)
        {
            var result = new DatabaseSchemaReader.DataSchema.DatabaseSchema(string.Empty, DatabaseSchemaReader.DataSchema.SqlType.SqlServer);
            foreach (var map in config.Maps)
            {
                var table = new DatabaseSchemaReader.DataSchema.DatabaseTable();
                table.Name = map.Table;
                foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne))
                {
                    table.Columns.Add(new DatabaseSchemaReader.DataSchema.DatabaseColumn { ForeignKeyTableName = config.GetMap(column.Value.Type).Table, Name = column.Value.DbName });
                }

                result.Tables.Add(table);
            }

            return result;
        }
    }

    public class CustomConfig : DefaultConfiguration
    {
        public CustomConfig()
            : base(new TopHat.Engine.SqlServerEngine(), string.Empty)
        {
            this.AddNamespaceOf<TopHat.Tests.TestDomain.Post>();
        }
    }
}
