namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class BulkUpdateWriter : BaseWriter, IBulkUpdateWriter {
        public BulkUpdateWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) { }

        public SqlWriterResult GenerateBulkSql<T>(Action<T> updateAction, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            var sql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();
            var map = this.Configuration.GetMap<T>();

            // run the update
            var entity = new T();
            ((ISetLogger)entity).EnableSetLogging();
            updateAction(entity);

            // find the set properties
            var setLogger = (ISetLogger)entity;
            var setProps = setLogger.GetSetProperties();
            if (!setProps.Any()) {
                return new SqlWriterResult(string.Empty, parameters);
            }

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" set ");

            foreach (var updatedProp in setProps) {
                var column = map.Columns[updatedProp];
                this.Dialect.AppendQuotedName(sql, column.DbName);
                var paramName = "@" + updatedProp;
                var propertyValue = map.GetColumnValue(entity, column);
                if (propertyValue == null) {
                    parameters.Add(paramName, null);
                }
                else {
                    parameters.Add(paramName, this.GetValueOrPrimaryKey(column, propertyValue));
                }

                sql.Append(" = ");
                sql.Append(paramName);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);

            if (predicates != null && predicates.Any()) {
                var whereClauseWriter = new WhereClauseWriter(this.Dialect, this.Configuration);
                var whereResult = whereClauseWriter.GenerateSql(predicates, null, parameters);
                if (whereResult.FetchTree != null && whereResult.FetchTree.Children.Any()) {
                    throw new NotImplementedException("Dashing does not currently support where clause across tables in an update");
                }

                sql.Append(whereResult.Sql);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}