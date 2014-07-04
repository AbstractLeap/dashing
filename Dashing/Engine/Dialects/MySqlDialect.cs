namespace Dashing.Engine.Dialects {
    using System.Text;

    public class MySqlDialect : SqlDialectBase {
        public MySqlDialect()
            : base('`', '`') { }

        protected override void AppendAutoGenerateModifier(StringBuilder sql) {
            sql.Append(" auto_increment");
        }

        public override string WriteDropTableIfExists(string tableName)
        {
            var sql = new StringBuilder("drop table if exists ");
            this.AppendQuotedName(sql, tableName);
            return sql.ToString();
        }

        public override string GetIdSql() {
            return "SELECT LAST_INSERT_ID() id";
        }

        public override void ApplyPaging(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
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