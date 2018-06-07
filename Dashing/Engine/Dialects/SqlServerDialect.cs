namespace Dashing.Engine.Dialects {
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;
    using Dashing.Versioning;

    public class SqlServerDialect : SqlDialectBase {
        private const char DotCharacter = '.';

        public SqlServerDialect()
            : base('[', ']') {
        }

        public override void AppendQuotedTableName(StringBuilder sql, IMap map) {
            if (map.Schema != null) {
                this.AppendQuotedName(sql, map.Schema);
                sql.Append(DotCharacter);
            }

            this.AppendQuotedName(sql, map.Table);
        }

        protected override void AppendAutoGenerateModifier(StringBuilder sql, IColumn column) {
            if (column.Type == typeof(Guid)) {
                sql.Append(" DEFAULT NEWSEQUENTIALID()");
            }
            else {
                sql.Append(" identity(1,1)");
            }
        }

        protected override void AppendColumnType(StringBuilder sql, IColumn column)
        {
            if (column.Map.Type.IsVersionedEntity() && column.Name == nameof(IVersionedEntity<string>.SessionUser)) {
                return;
            }

            base.AppendColumnType(sql, column);
        }

        protected override void AppendColumnProperties(StringBuilder sql, IColumn column, bool scriptDefault)
        {
            if (column.Map.Type.IsVersionedEntity()) {
                var spec = this.GetColumnSpecification(column);
                switch(column.Name) {
                    case nameof(IVersionedEntity<string>.SessionUser): // string doesn't matter here
                        sql.Append($" as (cast(SESSION_CONTEXT(N'UserId') as {spec.DbTypeName}))");
                        return;

                    case nameof(IVersionedEntity<string>.CreatedBy): // string doesn't matter here
                        sql.Append($" NULL DEFAULT (cast(SESSION_CONTEXT(N'UserId') as {spec.DbTypeName}))");
                        return;

                    case nameof(IVersionedEntity<string>.SysStartTime):
                        sql.Append(" GENERATED ALWAYS AS ROW START HIDDEN DEFAULT GETUTCDATE()");
                        return;

                    case nameof(IVersionedEntity<string>.SysEndTime):
                        sql.Append(" GENERATED ALWAYS AS ROW END HIDDEN DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999')")
                           .Append(", PERIOD FOR SYSTEM_TIME (")
                           .Append(nameof(IVersionedEntity<string>.SysStartTime))
                           .Append(", ")
                           .Append(nameof(IVersionedEntity<string>.SysEndTime))
                           .Append(")");
                        return;
                }
            }

            base.AppendColumnProperties(sql, column, scriptDefault);
        }

        public override void AppendCreateTableSuffix(StringBuilder sql, IMap map)
        {
            if (map.Type.IsVersionedEntity()) {
                sql.Append($" WITH (SYSTEM_VERSIONING = ON ( HISTORY_TABLE = {(string.IsNullOrWhiteSpace(map.Schema) ? "dbo" : map.Schema)}.");
                this.AppendQuotedName(sql, map.HistoryTable);
                sql.Append("))");
            }
        }

        public override string AddSystemVersioning(IMap to) {
            if (to.Type.IsVersionedEntity()) {
                var sql = new StringBuilder("alter table ");
                this.AppendQuotedTableName(sql, to);
                sql.Append(" set (system_versioning = ON ( history_table = ")
                   .Append(
                       string.IsNullOrWhiteSpace(to.Schema)
                           ? "dbo"
                           : to.Schema)
                   .Append(".");
                this.AppendQuotedName(sql, to.HistoryTable);
                sql.Append("))");
                return sql.ToString();
            }

            return string.Empty;
        }

        public override ColumnSpecification GetColumnSpecification(IColumn column) {
            switch (column.DbType) {
                case DbType.Boolean:
                    return new ColumnSpecification { DbTypeName = "bit" };

                case DbType.Guid:
                    return new ColumnSpecification { DbTypeName = "uniqueidentifier" };

                case DbType.Object:
                    return new ColumnSpecification { DbTypeName = "sql_variant" };

                default:
                    return base.GetColumnSpecification(column);
            }
        }

        public override DbType GetTypeFromString(string name, int? length, int? precision) {
            switch (name) {
                case "bit":
                    return DbType.Boolean;

                case "datetime2":
                    return DbType.DateTime2;

                case "uniqueidentifier":
                    return DbType.Guid;

                case "sql_variant":
                    return DbType.Object;

                default:
                    return base.GetTypeFromString(name, length, precision);
            }
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            return "EXEC sp_RENAME '" + toColumn.Map.Table + "." + fromColumn.DbName + "', '" + toColumn.DbName + "', 'COLUMN'";
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            var sql = new StringBuilder();

            // drop a column constraint if need be
            if (!string.IsNullOrEmpty(fromColumn.GetDefault(this)) && !fromColumn.IsPrimaryKey && !fromColumn.IsAutoGenerated
                && (string.IsNullOrEmpty(toColumn.GetDefault(this)) || fromColumn.GetDefault(this) != toColumn.GetDefault(this))) {
                sql.AppendLine(this.OnBeforeDropColumn(fromColumn));
            }

            // alter the column
            sql.Append("alter table ");
            this.AppendQuotedTableName(sql, toColumn.Map);
            sql.Append(" alter column ");
            this.AppendColumnSpecification(sql, toColumn, false);

            // add a column constraint if need be
            if (!string.IsNullOrEmpty(toColumn.GetDefault(this)) && !toColumn.IsPrimaryKey && !toColumn.IsAutoGenerated) {
                sql.AppendLine(";");
                sql.Append("alter table ");
                this.AppendQuotedTableName(sql, toColumn.Map);
                sql.Append(" add default (");
                sql.Append(toColumn.GetDefault(this));
                sql.Append(") for");
                this.AppendQuotedName(sql, toColumn.DbName);
            }

            return sql.ToString();
        }

        public override string DropForeignKey(ForeignKey foreignKey) {
            var sql = new StringBuilder("alter table ");
            this.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" drop constraint ");
            this.AppendQuotedName(sql, this.GetForeignKeyName(foreignKey));
            return sql.ToString();
        }

        public override string DropIndex(Index index) {
            var sql = new StringBuilder("drop index ");
            this.AppendQuotedName(sql, this.GetIndexName(index));
            sql.Append(" on ");
            this.AppendQuotedTableName(sql, index.Map);
            return sql.ToString();
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (skip == 0) {
                // query starts with SELECT so insert top (X) there
                sql.Insert(6, " top (@take)");
                return;
            }

            // now we have take and skip - we'll do the recursive CTE thingy
            sql.Insert(6, " ROW_NUMBER() OVER (" + orderClause + ") as RowNum,");
            sql.Insert(0, "select * from (");

            // see MySqlDialect for explanation of the crazy number 18446744073709551615
            sql.Append(
                ") as pagetable where pagetable.RowNum between @skip + 1 and " + (take > 0 ? "@skip + @take" : "18446744073709551615")
                + " order by pagetable.RowNum");
        }

        public override string CreateIndex(Index index) {
            var statement = base.CreateIndex(index);
            if (index.IsUnique && index.Columns.Any(c => c.IsNullable)) {
                var whereClause = new StringBuilder();
                whereClause.Append(" where ");
                bool first = true;
                foreach (var column in index.Columns.Where(c => c.IsNullable)) {
                    if (!first) {
                        whereClause.Append(" and ");
                    }

                    this.AppendQuotedName(whereClause, column.DbName);
                    whereClause.Append(" is not null");
                    first = false;
                }
                statement += whereClause.ToString();
            }

            return statement;
        }

        public override string ChangeTableName(IMap @from, IMap to) {
            var sql = new StringBuilder("EXEC sp_RENAME ");
            this.AppendQuotedTableName(sql, from);
            sql.Append(", ");
            this.AppendQuotedTableName(sql, to);
            return sql.ToString();
        }

        public override string CheckDatabaseExists(string databaseName) {
            return "SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '" + databaseName + "' OR name = '" + databaseName + "')";
        }

        public override string GetIdSql() {
            return string.Empty;
        }

        public override void AppendIdOutput(StringBuilder sql, IMap map) {
            sql.Append("output inserted.")
               .Append(this.BeginQuoteCharacter)
               .Append(map.PrimaryKey.DbName)
               .Append(this.EndQuoteCharacter);
        }

        public override void AppendForUpdateUsingTableHint(StringBuilder tableSql) {
            tableSql.Append(" with (rowlock, xlock)");
        }

        public override void AppendForUpdateOnQueryFinish(StringBuilder sql) {
        }

        public override string OnBeforeDropColumn(IColumn column) {
            var commandName = "@OBDCommand" + Guid.NewGuid().ToString("N");
            var sb =
                new StringBuilder("declare ").Append(commandName)
                                             .AppendLine(" nvarchar(1000);")
                                             .Append("select ")
                                             .Append(commandName)
                                             .Append(" = 'ALTER TABLE ");
            this.AppendQuotedTableName(sb, column.Map);
            sb.Append(" drop constraint ' + d.name ").Append(@"from sys.tables t   
                          join    sys.default_constraints d       
                           on d.parent_object_id = t.object_id  
                          join    sys.columns c      
                           on c.object_id = t.object_id      
                            and c.column_id = d.parent_column_id
                         where t.name = '");
            sb.Append(column.Map.Table).Append("' and c.name = '").Append(column.DbName).AppendLine("';");
            sb.Append("execute(").Append(commandName).AppendLine(");");
            return sb.ToString();
        }
    }
}