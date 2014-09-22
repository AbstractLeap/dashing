namespace Dashing.Engine.DDL {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class CreateTableWriter : ICreateTableWriter {
        private readonly ISqlDialect dialect;

        public CreateTableWriter(ISqlDialect dialect) {
            if (dialect == null) {
                throw new ArgumentNullException("dialect");
            }

            this.dialect = dialect;
        }

        public string CreateTable(IMap map) {
            var sql = new StringBuilder();

            sql.Append("create table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" (");

            this.dialect.AppendColumnSpecification(sql, map.PrimaryKey);

            foreach (var column in map.OwnedColumns(true).Where(c => !c.IsPrimaryKey)) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }

            sql.Append(")");
            return sql.ToString();
        }

        public IEnumerable<string> CreateForeignKeys(IMap map) {
            return
                this.CreateForeignKeys(
                    map.Columns.Where(
                        c =>
                        !c.Value.IsIgnored && c.Value.Relationship == RelationshipType.ManyToOne).Select(m => m.Value));
        }

        private void CreateForeignKeyForColumn(
            IColumn column,
            List<string> sqlStatements) {
            var sql = new StringBuilder();
            var map = column.Map;
            sql.Append("alter table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" add constraint fk" + map.Type.Name + "_" + column.Name);
            sql.Append(" foreign key (");
            this.dialect.AppendQuotedName(sql, column.DbName);
            sql.Append(") references ");
            this.dialect.AppendQuotedTableName(sql, column.ParentMap);
            sql.Append("(");
            this.dialect.AppendQuotedName(sql, column.ParentMap.PrimaryKey.DbName);
            sql.Append(")");
            sqlStatements.Add(sql.ToString());
        }

        public IEnumerable<string> CreateForeignKeys(IEnumerable<IColumn> columns) {
            var sqlStatements = new List<string>();
            foreach (var column in columns) {
                this.CreateForeignKeyForColumn(column, sqlStatements);
            }

            return sqlStatements;
        }

        public IEnumerable<string> CreateIndexes(IMap map) {
            var indexIdx = 0;

            foreach (var index in map.Indexes) {
                var sql = new StringBuilder(128);
                sql.Append("create ");
                if (index.IsUnique) {
                    sql.Append("unique ");
                }

                sql.Append("index ");
                sql.Append("idx" + map.Type.Name + "_" + ++indexIdx);
                sql.Append(" on ");
                this.dialect.AppendQuotedTableName(sql, map);
                sql.Append(" (");
                foreach (var column in index.Columns) {
                    this.dialect.AppendQuotedName(sql, column.DbName);
                    sql.Append(", ");
                }

                sql.Remove(sql.Length - 2, 2);
                sql.Append(")");
                yield return sql.ToString();
            }
        }
    }
}