namespace TopHat.Engine {
    using System.Text;

    using TopHat.Configuration;

    public interface ISqlDialect {
        void AppendQuotedTableName(StringBuilder sql, IMap map);

        void AppendQuotedName(StringBuilder sql, string name);

        void AppendColumnSpecification(StringBuilder sql, IColumn column);

        void AppendEscaped(StringBuilder sql, string s);

        string WriteDropTableIfExists(string tableName);

        string GetIdSql();
    }
}