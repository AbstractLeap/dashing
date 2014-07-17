namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    internal class UpdateWriter : BaseWriter, IUpdateWriter {
        public UpdateWriter(ISqlDialect dialect, IConfiguration config)
            : this(dialect, new WhereClauseWriter(dialect, config), config) {
        }

        public UpdateWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) {
        }

        public SqlWriterResult GenerateSql<T>(IEnumerable<T> entities) {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            var paramIdx = 0;

            // we'll chuck these all in one query
            foreach (var entity in entities) {
                this.GenerateUpdateSql(entity, sql, parameters, ref paramIdx);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }

        private void GenerateUpdateSql<T>(T entity, StringBuilder sql, DynamicParameters parameters, ref int paramIdx) {
            var map = this.Configuration.GetMap<T>();
            Dictionary<string, object> dirtyProperties;

            // establish the dirty properties, either using a TrackedEntityInspector, or just assume everything is dirty
            if (TrackedEntityInspector<T>.IsTracked(entity)) {
                var inspector = new TrackedEntityInspector<T>(entity);
                if (!inspector.IsDirty() || inspector.HasOnlyDirtyCollections()) {
                    return;
                }

                dirtyProperties = inspector.DirtyProperties.ToDictionary(p => p, p => inspector.NewValues[p]);
            }
            else {
                dirtyProperties =
                    map.OwnedColumns(true)
                       .Where(c => !c.IsPrimaryKey)
                       .ToDictionary(
                            c => c.Name,
                            c => typeof(T).GetProperty(c.Name).GetValue(entity) // TODO: map does this for you?
                       );
            }

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, this.Configuration.GetMap<T>());

            // set each of the fields to the new value
            sql.Append(" set ");
            foreach (var property in dirtyProperties) {
                var paramName = "@p_" + ++paramIdx;
                parameters.Add(paramName, property.Value);
                this.Dialect.AppendQuotedName(sql, map.Columns[property.Key].DbName);
                sql.Append(" = ");
                sql.Append(paramName);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);

            // limit the update to the primary key = the entity id
            sql.Append(" where ");
            this.Dialect.AppendQuotedName(sql, this.Configuration.GetMap<T>().PrimaryKey.DbName);
            sql.Append(" = ");
            var idParamName = "@p_" + ++paramIdx;
            parameters.Add(idParamName, this.Configuration.GetMap<T>().GetPrimaryKeyValue(entity));
            sql.Append(idParamName);


            // FIN
            sql.Append(";");

            //// TODO Should we update collections here or is that the users job? Guess we should do ManyToMany tho
        }

        public SqlWriterResult GenerateBulkSql<T>(T updateClass, IEnumerable<Expression<Func<T, bool>>> predicates) {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            var map = this.Configuration.GetMap<T>();

            var interfaceUpdateClass = updateClass as IUpdateClass;
            if (interfaceUpdateClass.UpdatedProperties.IsEmpty()) {
                return new SqlWriterResult(string.Empty, parameters);
            }

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" set ");

            foreach (var updatedProp in interfaceUpdateClass.UpdatedProperties) {
                var column = map.Columns[updatedProp];
                this.Dialect.AppendQuotedName(sql, column.DbName);
                var paramName = "@" + updatedProp;
                parameters.Add(paramName, map.GetColumnValue(updateClass, column));
                sql.Append(" = ");
                sql.Append(paramName);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);

            if (predicates != null && predicates.Any()) {
                var whereResult = this.WhereClauseWriter.GenerateSql(predicates, null);
                if (whereResult.FetchTree != null && whereResult.FetchTree.Children.Any()) {
                    throw new NotImplementedException("Dashing does not currently support where clause across tables in an update");
                }

                parameters.AddDynamicParams(whereResult.Parameters);
                sql.Append(whereResult.Sql);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}