namespace Dashing.SchemaReading {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Database {
        private IEnumerable<TableDto> tables;

        private IEnumerable<ColumnDto> columns;

        private IEnumerable<IndexDto> indexes;

        private IEnumerable<ForeignKeyDto> foreignKeys;

        private ILookup<string, TableDto> tablesByName;

        private ILookup<string, ColumnDto> columnsByTable;

        private Dictionary<ColumnNameKey, ColumnDto> columnsByTableAndName;

        private ILookup<string, ForeignKeyDto> foreignKeysByTable;

        private ILookup<string, IndexDto> indexesByTable;

        private Dictionary<ColumnNameKey, ForeignKeyDto> foreignKeysByTableAndColumnName;

        public Database(
            IEnumerable<TableDto> tables,
            IEnumerable<ColumnDto> columns,
            IEnumerable<IndexDto> indexes,
            IEnumerable<ForeignKeyDto> foreignKeys) {
            this.tables = tables ?? new List<TableDto>();
            this.columns = columns ?? new List<ColumnDto>();
            this.indexes = indexes ?? new List<IndexDto>();
            this.foreignKeys = foreignKeys ?? new List<ForeignKeyDto>();
            this.InitLookups();
        }

        private void InitLookups() {
            this.tablesByName = this.tables.ToLookup(t => t.Name.ToLowerInvariant());
            this.columnsByTable = this.columns.ToLookup(t => t.TableName.ToLowerInvariant());
            this.foreignKeysByTable = this.foreignKeys.ToLookup(t => t.TableName.ToLowerInvariant());
            this.indexesByTable = this.indexes.ToLookup(t => t.TableName.ToLowerInvariant());
            this.columnsByTableAndName = this.columns.ToDictionary(t => new ColumnNameKey { TableName = t.TableName, ColumnName = t.Name });
            this.foreignKeysByTableAndColumnName = this.foreignKeys.ToDictionary(t => new ColumnNameKey { TableName = t.TableName, ColumnName = t.ColumnName });
        }

        public IEnumerable<TableDto> Tables {
            get {
                return this.tables;
            }
        } 

        public TableDto GetTableByName(string name, string schema = null) {
            var lowerName = name.ToLowerInvariant();
            if (!this.tablesByName.Contains(lowerName)) {
                return null;
            }

            var matchingTables = this.tablesByName[lowerName];
            if (schema == null) {
                if (matchingTables.Count() > 1) {
                    throw new InvalidOperationException(
                        string.Format("There are multiple tables available with name {0}. Please specify the schema.", name));
                }

                return matchingTables.First();
            }
            
            return matchingTables.FirstOrDefault(t => t.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ColumnDto> GetColumnsForTable(string tableName) {
            var lowerName = tableName.ToLowerInvariant();
            if (!this.columnsByTable.Contains(lowerName)) {
                return new ColumnDto[0];
            }

            return this.columnsByTable[lowerName];
        }

        public ColumnDto GetColumnByName(string columnName, string tableName) {
            var key = new ColumnNameKey { TableName = tableName, ColumnName = columnName };
            if (this.columnsByTableAndName.ContainsKey(key)) {
                return this.columnsByTableAndName[key];
            }

            return null;
        }

        public IEnumerable<ForeignKeyDto> GetForeignKeysForTable(string tableName) {
            var lowerName = tableName.ToLowerInvariant();
            if (!this.foreignKeysByTable.Contains(lowerName)) {
                return new ForeignKeyDto[0];
            }

            return this.foreignKeysByTable[lowerName];
        }

        public IEnumerable<IndexDto> GetIndexesForTable(string tableName) {
            var lowerName = tableName.ToLowerInvariant();
            if (!this.indexesByTable.Contains(lowerName)) {
                return new IndexDto[0];
            }

            return this.indexesByTable[lowerName];
        }

        public ForeignKeyDto GetForeignKeyForColumn(string columnName, string tableName) {
            var key = new ColumnNameKey { TableName = tableName, ColumnName = columnName };
            if (this.foreignKeysByTableAndColumnName.ContainsKey(key)) {
                return this.foreignKeysByTableAndColumnName[key];
            }

            return null;
        }

        class ColumnNameKey {
            public string TableName { get; set; }

            public string ColumnName { get; set; }

            public override int GetHashCode() {
                unchecked {
                    return this.TableName.ToLowerInvariant().GetHashCode() * this.ColumnName.ToLowerInvariant().GetHashCode();
                }
            }

            public override bool Equals(object obj) {
                if (obj == null) {
                    return false;
                }

                var otherKey = obj as ColumnNameKey;
                if (otherKey == null) {
                    return false;
                }

                return this.TableName.Equals(otherKey.TableName, StringComparison.OrdinalIgnoreCase)
                       && this.ColumnName.Equals(otherKey.ColumnName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}