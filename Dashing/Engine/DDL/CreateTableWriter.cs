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
                if (column.Relationship == RelationshipType.Owned) {
                    var ownedMap = map.Configuration.GetMap(column.Type);
                    if (ownedMap == null) {
                        throw new Exception($"Could not locate map for {column.Type}. It might need adding to the configuration");
                    }

                    foreach (var ownedColumn in ownedMap.OwnedColumns(true)) {
                        AddColumn(ownedColumn);
                    }
                }
                else {
                    AddColumn(column);
                }
            }

            sql.Append(")");
            this.dialect.AppendCreateTableSuffix(sql, map);
            return sql.ToString();

            void AddColumn(IColumn column) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }
        }

        public IEnumerable<string> CreateForeignKeys(IMap map) {
            return this.CreateForeignKeys(map.ForeignKeys);
        }

        private string CreateForeignKey(ForeignKey foreignKey) {
            var sql = new StringBuilder();
            sql.Append("alter table ");
            this.dialect.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" add constraint ").Append(this.dialect.GetForeignKeyName(foreignKey)).Append(" foreign key (");
            this.dialect.AppendQuotedName(sql, foreignKey.ChildColumn.DbName);
            sql.Append(") references ");
            this.dialect.AppendQuotedTableName(sql, foreignKey.ParentMap);
            sql.Append("(");
            this.dialect.AppendQuotedName(sql, foreignKey.ParentMap.PrimaryKey.DbName);
            sql.Append(")");
            return sql.ToString();
        }

        public IEnumerable<string> CreateForeignKeys(IEnumerable<ForeignKey> foreignKeys) {
            return foreignKeys.Select(f => this.dialect.CreateForeignKey(f));
        }

        public IEnumerable<string> CreateIndexes(IMap map) {
            return this.CreateIndexes(map.Indexes);
        }

        public IEnumerable<string> CreateIndexes(IEnumerable<Index> indexes) {
            foreach (var index in indexes) {
                yield return this.dialect.CreateIndex(index);
            }
        }
    }
}