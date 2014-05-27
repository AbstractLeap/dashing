namespace TopHat.Engine.DDL {
    using System;
    using System.Linq;
    using System.Text;

    using TopHat.Configuration;

    public class CreateTableWriter {
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

            foreach (var column in map.Columns.Values.Where(c => !c.IsPrimaryKey && !c.IsIgnored && (c.Relationship == RelationshipType.None || c.Relationship == RelationshipType.OneToMany))) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }

            sql.Append(")");
            return sql.ToString();
        }
    }
}