namespace TopHat.Engine {
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
    }
}