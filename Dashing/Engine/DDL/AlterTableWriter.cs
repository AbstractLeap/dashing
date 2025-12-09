namespace Dashing.Engine.DDL {
    using System;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    public class AlterTableWriter : IAlterTableWriter {
        private readonly ISqlDialect dialect;

        public AlterTableWriter(ISqlDialect dialect) {
            this.dialect = dialect;
        }

        public string AddColumn(params IColumn[] columns) {
            if (columns == null || columns.Length == 0) {
                return string.Empty;
            }

            var map = columns[0].Map;
            var sql = new StringBuilder("alter table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" add ");
            foreach (var columnEntry in columns.AsSmartEnumerable()) {
                var column = columnEntry.Value;
                if (column.Map.Type != map.Type) {
                    throw new InvalidOperationException("The columns must be for the same type");
                }

                this.dialect.AppendColumnSpecification(sql, column);
                if (!columnEntry.IsLast) {
                    sql.Append(", ");
                }
            }

            return sql.ToString();
        }

        public string DropColumn(IColumn column) {
            var beforeSql = this.dialect.OnBeforeDropColumn(column);
            var sql = new StringBuilder("alter table ");
            this.dialect.AppendQuotedTableName(sql, column.Map);
            sql.Append(" drop column ");
            this.dialect.AppendQuotedName(sql, column.DbName);
            return beforeSql + sql;
        }

        public string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            return this.dialect.ChangeColumnName(fromColumn, toColumn);
        }

        public string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            return this.dialect.ModifyColumn(fromColumn, toColumn);
        }

        public string DropForeignKey(ForeignKey foreignKey) {
            return this.dialect.DropForeignKey(foreignKey);
        }

        public string DropIndex(Dashing.Configuration.Index index) {
            return this.dialect.DropIndex(index);
        }

        public string RenameTable(IMap @from, IMap to) {
            return this.dialect.ChangeTableName(@from, to);
        }

        public string AddSystemVersioning(IMap to) {
            return this.dialect.AddSystemVersioning(to);
        }
    }
}