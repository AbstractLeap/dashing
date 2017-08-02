namespace Dashing.SchemaReading {
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    abstract class BaseSchemaReader : ISchemaReader {
        public Database Read(IDbConnection dbConnection, string databaseName) {
            // get the data
            return new Database(
                this.ReadTables(dbConnection, databaseName),
                this.ReadColumns(dbConnection, databaseName),
                this.ReadIndexes(dbConnection, databaseName),
                this.ReadForeignKeys(dbConnection, databaseName));
        }

        protected virtual IEnumerable<ForeignKeyDto> ReadForeignKeys(IDbConnection dbConnection, string databaseName) {
            var sql = this.GetForeignKeySql(databaseName);
            return dbConnection.Query<ForeignKeyDto>(sql, new { DatabaseName = databaseName });
        }

        protected abstract string GetForeignKeySql(string databaseName);

        protected virtual IEnumerable<IndexDto> ReadIndexes(IDbConnection dbConnection, string databaseName) {
            var sql = this.GetIndexSql();
            return dbConnection.Query<IndexDto>(sql, new { DatabaseName = databaseName });
        }

        protected abstract string GetIndexSql();

        protected virtual IEnumerable<ColumnDto> ReadColumns(IDbConnection dbConnection, string databaseName) {
            var sql = this.GetColumnSql();
            return dbConnection.Query<ColumnDto>(sql, new { DatabaseName = databaseName });
        }

        protected abstract string GetColumnSql();

        protected virtual IEnumerable<TableDto> ReadTables(IDbConnection dbConnection, string databaseName) {
            var sql = this.GetTableSql();
            return dbConnection.Query<TableDto>(sql, new { DatabaseName = databaseName });
        }

        protected virtual string GetTableSql() {
            return "select TABLE_SCHEMA as [Schema], TABLE_NAME as [Name] from INFORMATION_SCHEMA.TABLES where TABLE_CATALOG = @DatabaseName";
        }
    }
}