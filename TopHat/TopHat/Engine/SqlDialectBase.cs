namespace TopHat.Engine {
    using System;
    using System.Data;
    using System.Text;

    using TopHat.Configuration;

    public class SqlDialectBase : ISqlDialect {
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

        public virtual void AppendQuotedTableName(StringBuilder sql, IMap map) {
            this.AppendQuotedName(sql, map.Table);
        }

        public virtual void AppendColumnSpecification(StringBuilder sql, IColumn column) {
            this.AppendQuotedName(sql, column.DbName);
            sql.Append(" ");
            sql.Append(this.TypeName(column.DbType));

            if (this.TypeTakesLength(column.DbType)) {
                this.AppendLength(sql, column.Length);
            }

            if (this.TypeTakesPrecisionAndScale(column.DbType)) {
                this.AppendPrecisionAndScale(sql, column.Precision, column.Scale);
            }

            sql.Append(column.IsNullable ? " null" : " not null");

            if (column.IsPrimaryKey) {
                sql.Append(" primary key");
            }
        }

        public virtual void AppendEscaped(StringBuilder sql, string s) {
            sql.Append(s.Replace("'", "''"));
        }

        protected virtual void AppendLength(StringBuilder sql, ushort length) {
            sql.Append("(");
            sql.Append(length);
            sql.Append(")");
        }

        protected virtual void AppendPrecisionAndScale(StringBuilder sql, byte precision, byte scale) {
            sql.Append("(");
            sql.Append(precision);
            sql.Append(",");
            sql.Append(scale);
            sql.Append(")");
        }

        protected virtual string TypeName(DbType type) {
            switch (type) {
                case DbType.AnsiString:
                    return "varchar";

                case DbType.AnsiStringFixedLength:
                    return "char";

                case DbType.Binary:
                    return "varbinary(max)";

                case DbType.Boolean:
                    return "boolean";

                case DbType.Byte:
                    return "tinyint unsigned";

                case DbType.Currency:
                    return "money";

                case DbType.Date:
                    return "date";

                case DbType.DateTime:
                    return "datetime";

                case DbType.DateTime2:
                    return "datetime2";

                case DbType.DateTimeOffset:
                    return "datetimeoffset";

                case DbType.Decimal:
                    return "decimal";

                case DbType.Double:
                    return "float";

                case DbType.Guid:
                    return "char(36)";

                case DbType.Int16:
                    return "smallint";

                case DbType.Int32:
                    return "int";

                case DbType.Int64:
                    return "bigint";

                case DbType.Single:
                    return "real";

                case DbType.String:
                    return "nvarchar";

                case DbType.StringFixedLength:
                    return "nchar";

                case DbType.Time:
                    return "time";

                case DbType.SByte:
                    return "tinyint";

                case DbType.UInt16:
                    return "smallint unsigned";

                case DbType.UInt32:
                    return "int unsigned";

                case DbType.UInt64:
                    return "bigint unsigned";

                case DbType.Xml:
                    return "xml";

                default:
                    throw new NotSupportedException("Unsupported type " + type);
            }
        }

        protected virtual bool TypeTakesLength(DbType type) {
            switch (type) {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Binary:
                case DbType.String:
                case DbType.StringFixedLength:
                    return true;

                default:
                    return false;
            }
        }

        protected virtual bool TypeTakesPrecisionAndScale(DbType type) {
            switch (type) {
                case DbType.Decimal:
                    return true;

                default:
                    return false;
            }
        }
    }
}