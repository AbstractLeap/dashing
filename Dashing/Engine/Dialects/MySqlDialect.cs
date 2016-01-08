namespace Dashing.Engine.Dialects {
    using System.Text;

    using Dashing.Configuration;

    public class MySqlDialect : SqlDialectBase {
        public MySqlDialect()
            : base('`', '`') {
        }

        protected override void AppendAutoGenerateModifier(StringBuilder sql, IColumn column) {
            sql.Append(" auto_increment");
        }

        public override string WriteDropTableIfExists(string tableName) {
            var sql = new StringBuilder("drop table if exists ");
            this.AppendQuotedName(sql, tableName);
            return sql.ToString();
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, toColumn.Map);
            sql.Append(" change ");
            this.AppendQuotedName(sql, fromColumn.DbName);
            sql.Append(" ");
            this.AppendQuotedName(sql, toColumn.DbName);
            this.AppendColumnSpecificationWithoutName(sql, fromColumn);
            return sql.ToString();
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, toColumn.Map);
            sql.Append(" modify column ");
            this.AppendColumnSpecification(sql, toColumn);
            return sql.ToString();
        }

        public override string DropForeignKey(ForeignKey foreignKey) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" drop foreign key ");
            this.AppendQuotedName(sql, foreignKey.Name);
            return sql.ToString();
        }

        public override string DropIndex(Index index) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, index.Map);
            sql.Append(" drop index ");
            this.AppendQuotedName(sql, index.Name);
            return sql.ToString();
        }

        public override void AppendForUpdateUsingTableHint(StringBuilder tableSql) {
        }

        public override void AppendForUpdateOnQueryFinish(StringBuilder sql) {
            sql.Append(" for update");
        }

        public override string ChangeTableName(IMap @from, IMap to) {
            var sql = new StringBuilder("rename table ");
            this.AppendQuotedTableName(sql, from);
            sql.Append(" to ");
            this.AppendQuotedTableName(sql, to);
            return sql.ToString();
        }

        public override string CheckDatabaseExists(string databaseName) {
            return "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + databaseName + "'";
        }

        public override string GetIdSql() {
            return "SELECT LAST_INSERT_ID() id";
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (take > 0 && skip > 0) {
                sql.Append(" limit @skip, @take");
            }
            else if (take > 0) {
                sql.Append(" limit @take");
            }
            else if (skip > 0) {
                // yikes, limit is not optional so specify massive number 2^64-1
                sql.Append(" limit @skip, 18446744073709551615");
            }
        }
    }
}