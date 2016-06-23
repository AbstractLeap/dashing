namespace Dashing.Engine.Dialects {
    using System;
    using System.Data;
    using System.Text;

    using Dashing.Configuration;

    public class SqliteDialect : SqlDialectBase {
        public SqliteDialect()
            : base('"', '"') {
        }

        public override bool IgnoreMultipleDatabases {
            get {
                return true;
            }
        }

        protected override void AppendAutoGenerateModifier(StringBuilder sql, IColumn column) {
            // INT PRIMARY KEY is auto increment by default?!
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (take > 0 && skip > 0) {
                sql.Append(" limit @take offset @skip");
            }
            else if (take > 0) {
                sql.Append(" limit @take");
            }
            else if (skip > 0) {
                // yikes, limit is not optional so specify massive number 2^64-1
                sql.Append(" limit 18446744073709551615 offset @skip");
            }
        }

        public override string CreateForeignKey(ForeignKey foreignKey) {
            return string.Empty; // Not supported yet - needs to drop and recreate tables etc
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            throw new System.NotImplementedException();
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            throw new System.NotImplementedException();
        }

        public override string DropForeignKey(ForeignKey foreignKey) {
            throw new System.NotImplementedException();
        }

        public override string DropIndex(Index index) {
            throw new System.NotImplementedException();
        }

        public override void AppendForUpdateUsingTableHint(StringBuilder tableSql) {
            // sqlite doesn't support FOR UPDATE type locks
            throw new NotSupportedException();
        }

        public override void AppendForUpdateOnQueryFinish(StringBuilder sql) {
            // sqlite doesn't support FOR UPDATE type locks
            throw new NotSupportedException();
        }

        public override string ChangeTableName(IMap @from, IMap to) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, from);
            sql.Append(" rename to ");
            this.AppendQuotedTableName(sql, to);
            return sql.ToString();
        }

        public override string GetIdSql() {
            return "SELECT last_insert_rowid() AS id";
        }

        public override string CheckDatabaseExists(string databaseName) {
            return string.Empty;
        }

        public override string WriteDropTableIfExists(string tableName) {
            var sql = new StringBuilder("DROP TABLE IF EXISTS ");
            this.AppendEscaped(sql, tableName);
            return sql.ToString();
        }

        protected override string TypeName(DbType type) {
            switch (type) {
                case DbType.Int32:
                case DbType.Int64:
                    return "INTEGER"; // necessary for autoincrements to work properly!

                default:
                    return base.TypeName(type);
            }
        }
    }
}