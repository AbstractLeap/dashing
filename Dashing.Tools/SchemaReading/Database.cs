namespace Dashing.Tools.SchemaReading {
    using System.Collections.Generic;

    public class Database {
        private IEnumerable<TableDto> tables;

        private IEnumerable<ColumnDto> columns;

        private IEnumerable<IndexDto> indexes;

        private IEnumerable<ForeignKeyDto> foreignKeys;

        public IEnumerable<TableDto> Tables {
            get {
                return this.tables ?? new TableDto[0];
            }
            set {
                this.tables = value;
            }
        }

        public IEnumerable<ColumnDto> Columns {
            get {
                return this.columns ?? new ColumnDto[0];
            }
            set {
                this.columns = value;
            }
        }

        public IEnumerable<IndexDto> Indexes {
            get {
                return this.indexes ?? new IndexDto[0];
            }
            set {
                this.indexes = value;
            }
        }

        public IEnumerable<ForeignKeyDto> ForeignKeys {
            get {
                return this.foreignKeys ?? new ForeignKeyDto[0];
            }
            set {
                this.foreignKeys = value;
            }
        }
    }
}