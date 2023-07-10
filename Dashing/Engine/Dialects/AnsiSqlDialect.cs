namespace Dashing.Engine.Dialects {
    using System;
    using System.Data;
    using System.Text;

    using Dashing.Configuration;

    public class AnsiSqlDialect : SqlDialectBase {
        public override ColumnSpecification GetColumnSpecification(IColumn column) {
            switch (column.DbType) {
                case DbType.Binary:
                    return new ColumnSpecification { DbTypeName = "bit", Length = this.GetLength(column) };

                case DbType.Boolean:
                    return new ColumnSpecification { DbTypeName = "smallint unsigned" };

                case DbType.Byte:
                    return new ColumnSpecification { DbTypeName = "smallint unsigned" };

                case DbType.DateTime:
                case DbType.DateTime2:
                    return new ColumnSpecification { DbTypeName = "timestamp" };

                case DbType.DateTimeOffset:
                    return new ColumnSpecification { DbTypeName = "timestamptz" };

                case DbType.Double:
                    return new ColumnSpecification { DbTypeName = "double precision" };

                default:
                    return base.GetColumnSpecification(column);
            }
        }

        public override DbType GetTypeFromString(string name, int? length, int? precision) {
            switch (name) {
                case "bit":
                    return DbType.Binary;

                case "smallint unsigned":
                    return DbType.Byte;

                case "timestamp":
                    return DbType.DateTime2;

                case "double precision":
                    return DbType.Double;

                default:
                    return base.GetTypeFromString(name, length, precision);
            }
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            throw new InvalidOperationException("There is no Ansi-SQL way of changing a column name.");
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            throw new InvalidOperationException("There is no Ansi-SQL way of changing a column type.");
        }

        public override string DropForeignKey(ForeignKey foreignKey) {
            throw new InvalidOperationException("There is no Ansi-SQL way of dropping a foreign key.");
        }

        public override string DropIndex(Index index) {
            throw new InvalidOperationException("There is no Ansi-SQL way of dropping an index.");
        }

        public override void AppendForUpdateUsingTableHint(StringBuilder tableSql, bool skipLocked) {
            throw new NotImplementedException();
        }

        public override void AppendForUpdateOnQueryFinish(StringBuilder sql, bool skipLocked) {
            throw new NotImplementedException();
        }

        public override string ChangeTableName(IMap @from, IMap to) {
            throw new NotImplementedException();
        }

        public override string CheckDatabaseExists(string databaseName) {
            throw new InvalidOperationException("There is no Ansi-SQL way of creating a database");
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            throw new InvalidOperationException("There is no Ansi-SQL way of expressing an offset-limit / skip-take clause.");
        }
    }
}