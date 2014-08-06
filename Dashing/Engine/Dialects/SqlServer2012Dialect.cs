namespace Dashing.Engine.Dialects {
    using System.Text;

    public class SqlServer2012Dialect : SqlServerDialect {
        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (orderClause.Length == 0) {
                // Sql Server 2012 only supports offset with an order by clause
                base.ApplySkipTake(sql, orderClause, take, skip);
                return;
            }

            sql.Append(" offset " + (skip > 0 ? "@skip" : "0") + " rows");

            if (take > 0) {
                sql.Append(" fetch next @take rows only");
            }
        }
    }
}