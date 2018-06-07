namespace Dashing.SchemaReading {
    public class TableDto {
        public string Schema { get; set; }

        public string Name { get; set; }

        public TemporalType TemporalType { get; set; }

        public string HistorySchema { get; set; }

        public string HistoryName { get; set; }
    }
}