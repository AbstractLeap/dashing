namespace TopHat.Engine {
    using System.Text;

    using Dapper;

    using TopHat.Configuration;

    internal class DeleteWriter : BaseWriter, IEntitySqlWriter {
        public DeleteWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, new WhereClauseWriter(dialect, config), config) { }

        public DeleteWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) { }

        public SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            sql.Append("delete from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" where ");
            this.Dialect.AppendQuotedName(sql, map.PrimaryKey.DbName);
            sql.Append(" in (");
            var paramIdx = 0;
            foreach (var entity in query.Entities) {
                string paramName = "@p_" + ++paramIdx;
                sql.Append(paramName + ", ");
                parameters.Add(paramName, map.GetPrimaryKeyValue(entity));
            }

            sql.Remove(sql.Length - 2, 2); // remove trailing ,
            sql.Append(")");
            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}