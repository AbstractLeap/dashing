namespace Dashing.Engine.Dialects {
    using System.Data;
    using System.Text;

    using Dashing.Configuration;

    public class MySqlDialect : SqlDialectBase {
        public MySqlDialect()
            : base('`', '`') {
        }

        protected override void AppendAutoGenerateModifier(StringBuilder sql, IColumn column) {
            sql.Append(" auto_increment");
        }

        public override string WriteDropTableIfExists(string tableName) {
            var sql = new StringBuilder("drop table if exists ");
            this.AppendQuotedName(sql, tableName);
            return sql.ToString();
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, toColumn.Map);
            sql.Append(" change ");
            this.AppendQuotedName(sql, fromColumn.DbName);
            sql.Append(" ");
            this.AppendQuotedName(sql, toColumn.DbName);
            this.AppendColumnSpecificationWithoutName(sql, fromColumn);
            return sql.ToString();
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, toColumn.Map);
            sql.Append(" modify column ");
            this.AppendColumnSpecification(sql, toColumn);
            return sql.ToString();
        }

        public override string DropForeignKey(ForeignKey foreignKey) {
            var name = this.GetForeignKeyName(foreignKey);
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" drop foreign key ");
            this.AppendQuotedName(sql, name);
            return sql.ToString();
        }

        public override string DropIndex(Index index) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, index.Map);
            sql.Append(" drop index ");
            this.AppendQuotedName(sql, this.GetIndexName(index));
            return sql.ToString();
        }

        public override void AppendForUpdateUsingTableHint(StringBuilder tableSql) {
        }

        public override void AppendForUpdateOnQueryFinish(StringBuilder sql) {
            sql.Append(" for update");
        }

        protected override void AppendDefault(StringBuilder sql, IColumn column) {
            sql.Append(" default ").Append(column.GetDefault(this));
        }

        public override string ChangeTableName(IMap @from, IMap to) {
            var sql = new StringBuilder("rename table ");
            this.AppendQuotedTableName(sql, from);
            sql.Append(" to ");
            this.AppendQuotedTableName(sql, to);
            return sql.ToString();
        }

        public override string CheckDatabaseExists(string databaseName) {
            return "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + databaseName + "'";
        }

        public override string GetIdSql() {
            return "SELECT LAST_INSERT_ID() id";
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (take > 0 && skip > 0) {
                sql.Append(" limit @skip, @take");
            }
            else if (take > 0) {
                sql.Append(" limit @take");
            }
            else if (skip > 0) {
                // yikes, limit is not optional so specify massive number 2^64-1
                sql.Append(" limit @skip, 18446744073709551615");
            }
        }

        public override string DefaultFor(DbType dbType, bool isNullable) {
            switch (dbType) {
                case DbType.Guid:
                    return null; // not possible to autogenerate a guid in MySql

                case DbType.Time:
                    return "'00:00'";
            }

            return base.DefaultFor(dbType, isNullable);
        }

        public override DbType GetTypeFromString(string name, int? length, int? precision) {
            switch (name) {
                case "text":
                    return DbType.String;

                case "tinyint":
                    if (precision.HasValue && precision.Value == 1) {
                        // MySql stores bool as tinyint(1)
                        return DbType.Boolean;
                    }
                    break;
            }

            return base.GetTypeFromString(name, length, precision);
        }

        public override ColumnSpecification GetColumnSpecification(IColumn column) {
            switch (column.DbType) {
                case DbType.String:
                    return new ColumnSpecification { DbTypeName = "varchar", Length = this.GetLength(column) };

                case DbType.StringFixedLength:
                    return new ColumnSpecification { DbTypeName = "char", Length = this.GetLength(column) };
            }

            return base.GetColumnSpecification(column);
        }

        public override void UpdateColumnFromSpecification(IColumn column, ColumnSpecification specification) {
            if (specification.DbTypeName == "text") {
                specification.Length = -1; // if it's a text column then it comes back with a value of 65536 but actually we want a String with MaxLength
            }

            base.UpdateColumnFromSpecification(column, specification);
        }

        public override string GetForeignKeyName(ForeignKey foreignKey) {
            var name = base.GetForeignKeyName(foreignKey);
            if (name.Length > 64) {
                return name.Substring(0, 64);
            }

            return name;
        }

        public override string GetIndexName(Index index) {
            var name = base.GetIndexName(index);
            if (name.Length > 64) {
                return name.Substring(0, 64);
            }

            return name;
        }
    }
}