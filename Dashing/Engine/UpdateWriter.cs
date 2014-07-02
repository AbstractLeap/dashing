namespace Dashing.Engine {
    using System.Text;

    using Dapper;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;

    internal class UpdateWriter : BaseWriter, IEntitySqlWriter {
        public UpdateWriter(ISqlDialect dialect, IConfiguration config)
            : this(dialect, new WhereClauseWriter(dialect, config), config) { }

        public UpdateWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) { }

        public SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query) {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            var paramIdx = 0;

            // we'll chuck these all in one query
            foreach (var entity in query.Entities) {
                this.GenerateUpdateSql(entity, sql, parameters, ref paramIdx);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }

        private void GenerateUpdateSql<T>(T entity, StringBuilder sql, DynamicParameters parameters, ref int paramIdx) {
            ITrackedEntityInspector<T> inspector = new TrackedEntityInspector<T>(entity);

            if (!inspector.IsDirty() || inspector.HasOnlyDirtyCollections()) {
                return;
            }

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, this.Configuration.GetMap<T>());
            sql.Append(" set ");

            foreach (var property in inspector.DirtyProperties) {
                string paramName = "@p_" + ++paramIdx;
                parameters.Add(paramName, inspector.NewValues[property]);
                this.Dialect.AppendQuotedName(sql, this.Configuration.GetMap<T>().Columns[property].DbName);
                sql.Append(" = " + paramName);
            }

            sql.Append(" where ");
            this.Dialect.AppendQuotedName(sql, this.Configuration.GetMap<T>().PrimaryKey.DbName);
            sql.Append(" = ");
            string idParamName = "@p_" + ++paramIdx;
            parameters.Add(idParamName, this.Configuration.GetMap<T>().GetPrimaryKeyValue(entity));
            sql.Append(idParamName);

            sql.Append(";");

            // TODO Should we update collections here or is that the users job? Guess we should do ManyToMany tho
        }
    }
}