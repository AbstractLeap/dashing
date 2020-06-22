﻿namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class InsertWriter : BaseWriter, IInsertWriter {
        public InsertWriter(ISqlDialect dialect, IConfiguration config) : base(dialect, config) {
            this.dialect = dialect;
            this.configuration = config;
        }

        private static readonly ConcurrentDictionary<WriterQueryCacheKey, string> QueryCache = new ConcurrentDictionary<WriterQueryCacheKey, string>();

        private readonly ISqlDialect dialect;

        private readonly IConfiguration configuration;

        public string GenerateGetIdSql<T>() {
            return this.dialect.GetIdSql();
        }

        public SqlWriterResult GenerateSql<T>(T entity) {
            var map = this.configuration.GetMap<T>();
            var sql = QueryCache.GetOrAdd(new WriterQueryCacheKey(this.configuration, typeof(T)), t => this.ReallyGenerateSql(map, entity));
            var parameters = new DynamicParameters();
            this.GenerateValuesSpec(null, parameters, map, entity, false, true);
            return new SqlWriterResult(sql, parameters);
        }

        private string ReallyGenerateSql<T>(IMap<T> map, T entity) {
            var sql = new StringBuilder();
            this.GenerateInsertSpec(sql, map);
            this.GenerateColumnSpec(sql, map);
            this.GenerateOutputSpec(sql, map);
            this.GenerateValuesSpec(sql, null, map, entity, true, false);
            return sql.ToString();
        }

        private void GenerateOutputSpec(StringBuilder sql, IMap map) {
            if (map.PrimaryKey.IsAutoGenerated) {
                this.dialect.AppendIdOutput(sql, map);
            }

            sql.Append(" values ");
        }

        private void GenerateInsertSpec(StringBuilder sql, IMap map) {
            sql.Append("insert into ");
            this.dialect.AppendQuotedTableName(sql, map);
        }

        private void GenerateColumnSpec(StringBuilder sql, IMap map) {
            sql.Append(" (");

            foreach (var column in map.OwnedColumns(true)
                                      .Where(c => !(c.IsAutoGenerated || c.IsComputed))
                                      .OrderBy(k => k.Name)) {
                // TODO recursion down through owned types
                if (column.Relationship == RelationshipType.Owned) {
                    var ownedMap = GetOwnedMap(column);
                    foreach (var ownedColumn in ownedMap.OwnedColumns(true))  {
                        AddColumn(ownedColumn);
                    }
                }
                else {
                    AddColumn(column);
                }

            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append(") ");

            void AddColumn(IColumn column) {
                this.dialect.AppendQuotedName(sql, column.DbName);
                sql.Append(", ");
            }
        }

        private void GenerateValuesSpec<T>(StringBuilder sql, DynamicParameters parameters, IMap<T> map, T entity, bool generateSql, bool fillParams) {
            var paramIdx = 0;
            if (generateSql) {
                sql.Append("(");
            }

            foreach (var column in map.OwnedColumns(true)
                                      .Where(c => !(c.IsAutoGenerated || c.IsComputed))
                                      .OrderBy(k => k.Name)) {
                // TODO recursion down through owned types
                if (column.Relationship == RelationshipType.Owned) {
                    var ownedMap = BaseWriter.GetOwnedMap(column);
                    foreach (var ownedColumn in ownedMap.OwnedColumns(true)) {
                        GenerateColumnValueSpec(sql, parameters, ownedColumn, map.GetColumnValue(entity, column), generateSql, fillParams, paramIdx++);
                    }
                }
                else {
                    GenerateColumnValueSpec(sql, parameters, column, entity, generateSql, fillParams, paramIdx++);
                }
            }

            if (generateSql) {
                sql.Remove(sql.Length - 2, 2);
                sql.Append(")");
            }
        }

        private void GenerateColumnValueSpec(StringBuilder sql, DynamicParameters parameters, IColumn column, object entity, bool generateSql, bool fillParams, int paramIdx) {
            var paramName = "@p_" + paramIdx;
            if (generateSql) {
                sql.Append(paramName);
                sql.Append(", ");
            }

            if (!fillParams) {
                return;
            }

            if (entity == null) {
                parameters.Add(paramName, null, column.DbType);
            }
            else {
                if (column.Relationship == RelationshipType.None) {
                    parameters.Add(paramName, column.Map.GetColumnValue(entity, column), column.DbType);
                }
                else {
                    var relatedEntity = column.Map.GetColumnValue(entity, column);
                    if (relatedEntity != null) {
                        parameters.Add(
                            paramName,
                            this.configuration.GetMap(column.Type)
                                .GetPrimaryKeyValue(relatedEntity),
                            column.DbType);
                    }
                    else {
                        parameters.Add(paramName, null, column.DbType);
                    }
                }
            }
        }
    }
}