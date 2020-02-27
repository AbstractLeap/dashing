namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#if NETSTANDARD2_0
    using System.Reflection;
#endif

    using Dapper;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class UpdateWriter : BaseWriter, IUpdateWriter {
        public UpdateWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) {
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
            var trackedEntity = entity as ITrackedEntity; // this should never fail?!
            if (trackedEntity != null) {
                var dirtyProps = trackedEntity.GetDirtyProperties();
                if (!dirtyProps.Any()) {
                    return;
                }

                // TODO cache the property accessors
                dirtyProperties = dirtyProps.ToDictionary(p => p, p => typeof(T).GetProperty(p).GetValue(entity));
            }
            else {
                throw new InvalidOperationException("In order to Save entities you must fetch Tracked entities");
            }

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, this.Configuration.GetMap<T>());

            // set each of the fields to the new value
            sql.Append(" set ");
            foreach (var property in dirtyProperties) {
                var paramName = "@p_" + ++paramIdx;
                object paramValue;
                var mappedColumn = map.Columns[property.Key];

                var propertyValue = property.Value;
                if (propertyValue == null) {
                    paramValue = null;
                }
                else {
                    paramValue = this.GetValueOrPrimaryKey(mappedColumn, propertyValue);
                }

                // add the parameter
                parameters.Add(paramName, paramValue, mappedColumn.DbType);

                // finish up the set claus
                this.Dialect.AppendQuotedName(sql, mappedColumn.DbName);
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
    }
}