namespace Dashing.CodeGeneration.Weaving {
    using System.Data;

    using Dashing.Configuration;

    public class ColumnDefinition
    {
        public string Name { get; set; }

        public string TypeFullName { get; set; }

        public RelationshipType Relationship { get; set; }

        public string RelatedTypePrimarykeyName { get; set; }

        public string DbName { get; set; }

        public bool IsPrimaryKey { get; set; }

        public DbType DbType { get; set; }
    }
}