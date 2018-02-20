namespace Dashing.SchemaReading {
    public class ForeignKeyDto {
        public string TableName { get; set; }

        public string Name { get; set; }

        public string ColumnName { get; set; }

        public string ReferencedTableName { get; set; }

        public string ReferencedColumnName { get; set; }
    }
}