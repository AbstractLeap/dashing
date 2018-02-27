namespace Dashing.SchemaReading {
    public class IndexDto {
        public string TableName { get; set; }

        public string Name { get; set; }

        public string ColumnName { get; set; }

        /// <summary>
        ///     Indicates the position of this column in the index
        /// </summary>
        public int ColumnId { get; set; }

        public bool IsUnique { get; set; }
    }
}