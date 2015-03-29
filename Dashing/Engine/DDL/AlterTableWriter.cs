namespace Dashing.Engine.DDL {
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class AlterTableWriter : IAlterTableWriter {
        private readonly ISqlDialect dialect;

        public AlterTableWriter(ISqlDialect dialect) {
            this.dialect = dialect;
        }

        public string AddColumn(IColumn column) {
            var sql = new StringBuilder("alter table ");
            this.dialect.AppendQuotedTableName(sql, column.Map);
            sql.Append(" add ");
            this.dialect.AppendColumnSpecification(sql, column);
            return sql.ToString();
        }

        public string DropColumn(IColumn column) {
            var beforeSql = this.dialect.OnBeforeDropColumn(column);
            var sql = new StringBuilder("alter table ");
            this.dialect.AppendQuotedTableName(sql, column.Map);
            sql.Append(" drop column ");
            this.dialect.AppendQuotedName(sql, column.DbName);
            return beforeSql + sql.ToString();
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

        public string DropIndex(Index index) {
            return this.dialect.DropIndex(index);
        }

        public string RenameTable(IMap @from, IMap to) {
            return this.dialect.ChangeTableName(@from, to);
        }
    }
}