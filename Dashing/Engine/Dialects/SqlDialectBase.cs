namespace Dashing.Engine.Dialects {
    using System;
    using System.Data;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public abstract class SqlDialectBase : ISqlDialect {
        protected char BeginQuoteCharacter { get; set; }

        protected char EndQuoteCharacter { get; set; }

        public SqlDialectBase(char beginQuoteCharacter = '"', char endQuoteCharacter = '"') {
            this.BeginQuoteCharacter = beginQuoteCharacter;
            this.EndQuoteCharacter = endQuoteCharacter;
        }

        public void AppendQuotedName(StringBuilder sql, string name) {
            sql.Append(this.BeginQuoteCharacter);
            sql.Append(name);
            sql.Append(this.EndQuoteCharacter);
        }

        public virtual bool IgnoreMultipleDatabases {
            get {
                return false;
            }
        }

        public virtual void AppendQuotedTableName(StringBuilder sql, IMap map) {
            this.AppendQuotedName(sql, map.Table);
        }

        public virtual void AppendColumnSpecification(StringBuilder sql, IColumn column, bool scriptDefault = true) {
            this.AppendQuotedName(sql, column.DbName);
            sql.Append(" ");
            this.AppendColumnSpecificationWithoutName(sql, column, scriptDefault);
        }

        public virtual ColumnSpecification GetColumnSpecification(IColumn column) {
            var spec = new ColumnSpecification();
            switch (column.DbType) {
                case DbType.AnsiString:
                    spec.DbTypeName = "varchar";
                    spec.Length = this.GetLength(column);

                    break;
                case DbType.AnsiStringFixedLength:
                    spec.DbTypeName = "char";
                    spec.Length = this.GetLength(column);

                    break;
                case DbType.Binary:
                    spec.DbTypeName = "varbinary";
                    spec.Length = -1;

                    break;
                case DbType.Boolean:
                    spec.DbTypeName = "boolean";

                    break;
                case DbType.Byte:
                    spec.DbTypeName = "tinyint unsigned";

                    break;
                case DbType.Currency:
                    spec.DbTypeName = "money";

                    break;
                case DbType.Date:
                    spec.DbTypeName = "date";

                    break;
                case DbType.DateTime:
                    spec.DbTypeName = "datetime";

                    break;
                case DbType.DateTime2:
                    spec.DbTypeName = "datetime2";
                    spec.Precision = column.Precision;

                    break;
                case DbType.DateTimeOffset:
                    spec.DbTypeName = "datetimeoffset";

                    break;
                case DbType.Decimal:
                    spec.DbTypeName = "decimal";
                    spec.Precision = column.Precision;
                    spec.Scale = column.Scale;
                    break;
                case DbType.Double:
                    spec.DbTypeName = "float";

                    break;
                case DbType.Guid:
                    spec.DbTypeName = "char";
                    spec.Length = 36;

                    break;
                case DbType.Int16:
                    spec.DbTypeName = "smallint";

                    break;
                case DbType.Int32:
                    spec.DbTypeName = "int";

                    break;
                case DbType.Int64:
                    spec.DbTypeName = "bigint";

                    break;
                case DbType.Single:
                    spec.DbTypeName = "real";

                    break;
                case DbType.String:
                    spec.DbTypeName = "nvarchar";
                    spec.Length = this.GetLength(column);
                    break;
                case DbType.StringFixedLength:
                    spec.DbTypeName = "nchar";
                    spec.Length = this.GetLength(column);
                    break;
                case DbType.Time:
                    spec.DbTypeName = "time";

                    break;
                case DbType.SByte:
                    spec.DbTypeName = "tinyint";

                    break;
                case DbType.UInt16:
                    spec.DbTypeName = "smallint unsigned";

                    break;
                case DbType.UInt32:
                    spec.DbTypeName = "int unsigned";

                    break;
                case DbType.UInt64:
                    spec.DbTypeName = "bigint unsigned";

                    break;
                case DbType.Xml:
                    spec.DbTypeName = "xml";
                    break;

                default:
                    throw new NotSupportedException("Unsupported type " + column.DbType);
            }

            return spec;
        }

        public virtual void UpdateColumnFromSpecification(IColumn column, ColumnSpecification specification) {
            column.DbType = this.GetTypeFromString(specification.DbTypeName, specification.Length, specification.Precision);
            if (specification.Length.HasValue) {
                if (specification.Length == -1) {
                    column.MaxLength = true;
                }
                else {
                    column.Length = (ushort)specification.Length.Value;
                }
            }

            if (specification.Precision.HasValue) {
                column.Precision = specification.Precision.Value;
            }

            if (specification.Scale.HasValue) {
                column.Scale = specification.Scale.Value;
            }
        }

        protected int GetLength(IColumn column) {
            if (column.MaxLength) {
                return -1;
            }

            return column.Length;
        }

        protected virtual void AppendColumnSpecificationWithoutName(StringBuilder sql, IColumn column, bool scriptDefault = true) {
            this.AppendColumnType(sql, column);
            this.AppendColumnProperties(sql, column, scriptDefault);
        }

        protected virtual void AppendColumnType(StringBuilder sql, IColumn column) {
            var spec = this.GetColumnSpecification(column);
            this.AppendColumnType(sql, spec);
        }

        protected virtual void AppendColumnType(StringBuilder sql, ColumnSpecification spec) {
            sql.Append(spec.DbTypeName);
            if (spec.Length.HasValue) {
                sql.Append("(");
                if (spec.Length == -1) {
                    sql.Append("max");
                }
                else {
                    sql.Append(spec.Length);
                }

                sql.Append(")");
            }

            if (spec.Precision.HasValue && spec.Scale.HasValue) {
                this.AppendPrecisionAndScale(sql, spec.Precision.Value, spec.Scale.Value);
            }
            else if (spec.Precision.HasValue) {
                this.AppendPrecision(sql, spec.Precision.Value);
            }
        }

        protected virtual void AppendColumnProperties(StringBuilder sql, IColumn column, bool scriptDefault) {
            sql.Append(
                column.IsNullable
                    ? " null"
                    : " not null");

            if (scriptDefault && !string.IsNullOrEmpty(column.GetDefault(this)) && !column.IsPrimaryKey && !column.IsAutoGenerated) {
                this.AppendDefault(sql, column);
            }

            if (column.IsAutoGenerated) {
                this.AppendAutoGenerateModifier(sql, column);
            }

            if (column.IsPrimaryKey) {
                sql.Append(" primary key");
            }
        }

        protected virtual void AppendDefault(StringBuilder sql, IColumn column) {
            sql.Append(" default (").Append(column.GetDefault(this)).Append(")");
        }

        public virtual void AppendEscaped(StringBuilder sql, string s) {
            sql.Append(s.Replace("'", "''"));
        }

        protected virtual void AppendPrecisionAndScale(StringBuilder sql, byte precision, byte scale) {
            sql.Append("(");
            sql.Append(precision);
            sql.Append(",");
            sql.Append(scale);
            sql.Append(")");
        }

        protected virtual void AppendPrecision(StringBuilder sql, byte precision) {
            sql.Append("(")
               .Append(precision)
               .Append(")");
        }

        protected virtual void AppendAutoGenerateModifier(StringBuilder sql, IColumn column) {
            sql.Append(" generated always as identity");
        }

        public virtual DbType GetTypeFromString(string name, int? length, int? precision) {
            switch (name) {
                case "varchar":
                    return DbType.AnsiString;

                case "char":
                    return DbType.AnsiStringFixedLength;

                case "varbinary(max)":
                case "varbinary":
                    return DbType.Binary;

                case "boolean":
                case "bool":
                    return DbType.Boolean;

                case "tinyint unsigned":
                    return DbType.Byte;

                case "money":
                    return DbType.Currency;

                case "date":
                    return DbType.Date;

                case "datetime":
                    return DbType.DateTime;

                case "datetime2":
                    return DbType.DateTime2;

                case "datetimeoffset":
                    return DbType.DateTimeOffset;

                case "decimal":
                    return DbType.Decimal;

                case "float":
                    return DbType.Double;

                case "char(36)":
                    return DbType.Guid;

                case "smallint":
                    return DbType.Int16;

                case "int":
                    return DbType.Int32;

                case "bigint":
                    return DbType.Int64;

                case "real":
                    return DbType.Single;

                case "nvarchar":
                    return DbType.String;

                case "nchar":
                    return DbType.StringFixedLength;

                case "time":
                    return DbType.Time;

                case "tinyint":
                    return DbType.SByte;

                case "smallint unsigned":
                    return DbType.UInt16;

                case "int unsigned":
                    return DbType.UInt32;

                case "bigint unsigned":
                    return DbType.UInt64;

                case "xml":
                    return DbType.Xml;

                default:
                    throw new NotSupportedException("Unsupported type " + name);
            }
        }

        public virtual string DefaultFor(DbType dbType, bool isNullable) {
            switch (dbType) {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return isNullable ? null : string.Empty;
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return isNullable ? null : "0";
                case DbType.Binary:
                    return null;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Time:
                    return isNullable ? null : "current_timestamp";
                case DbType.Guid:
                    return isNullable ? null : "newid()";
                case DbType.DateTimeOffset:
                    return isNullable ? null : "sysdatetimeoffset()";
                case DbType.Object:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException("dbType", "Unknown Db Type for Default value resolution");
            }
        }

        public virtual string GetForeignKeyName(ForeignKey foreignKey) {
            var name = foreignKey.Name;
            if (string.IsNullOrWhiteSpace(name)) {
                name = "fk_" + foreignKey.ChildColumn.Map.Type.Name + "_" + foreignKey.ParentMap.Type.Name + "_" + foreignKey.ChildColumn.Name;
            }

            return name;
        }

        public virtual string GetIndexName(Index index) {
            var name = index.Name;
            if (string.IsNullOrWhiteSpace(name)) {
                name = "idx_" + index.Map.Type.Name + "_" + string.Join("_", index.Columns.Select(c => c.Name));
            }

            return name;
        }

        public virtual string AddSystemVersioning(IMap to) {
            return string.Empty;
        }

        public abstract string ChangeColumnName(IColumn fromColumn, IColumn toColumn);

        public abstract string ModifyColumn(IColumn fromColumn, IColumn toColumn);

        public abstract string DropForeignKey(ForeignKey foreignKey);

        public abstract string DropIndex(Index index);

        public virtual string CreateIndex(Index index) {
            var sql = new StringBuilder(128);
            sql.Append("create ");
            if (index.IsUnique) {
                sql.Append("unique ");
            }

            sql.Append("index ");
            this.AppendQuotedName(sql, this.GetIndexName(index));
            sql.Append(" on ");
            this.AppendQuotedTableName(sql, index.Map);
            sql.Append(" (");
            foreach (var column in index.Columns) {
                this.AppendQuotedName(sql, column.DbName);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append(")");
            return sql.ToString();
        }

        public virtual string CreateForeignKey(ForeignKey foreignKey) {
            var sql = new StringBuilder();
            sql.Append("alter table ");
            this.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" add constraint ").Append(this.GetForeignKeyName(foreignKey)).Append(" foreign key (");
            this.AppendQuotedName(sql, foreignKey.ChildColumn.DbName);
            sql.Append(") references ");
            this.AppendQuotedTableName(sql, foreignKey.ParentMap);
            sql.Append("(");
            this.AppendQuotedName(sql, foreignKey.ParentMap.PrimaryKey.DbName);
            sql.Append(")");
            return sql.ToString();
        }

        public abstract void AppendForUpdateUsingTableHint(StringBuilder tableSql, bool skipLocked);

        public abstract void AppendForUpdateOnQueryFinish(StringBuilder sql, bool skipLocked);

        public virtual void AppendCreateTableSuffix(StringBuilder sql, IMap map) {
            
        }

        public virtual string OnBeforeDropColumn(IColumn column) {
            return string.Empty;
        }

        public abstract string ChangeTableName(IMap @from, IMap to);

        public string CreateDatabase(string databaseName) {
            return "create database " + this.BeginQuoteCharacter + databaseName + this.EndQuoteCharacter;
        }

        public abstract string CheckDatabaseExists(string databaseName);

        public virtual string GetIdSql() {
            return "select @@identity id";
        }

        public virtual void AppendIdOutput(StringBuilder sql, IMap map) {

        }

        public virtual string WriteDropTableIfExists(string tableName) {
            var sql = new StringBuilder();
            sql.Append("if exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '");
            this.AppendEscaped(sql, tableName);
            sql.Append("') drop table ");
            this.AppendQuotedName(sql, tableName);
            return sql.ToString();
        }

        public abstract void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip);
    }
}