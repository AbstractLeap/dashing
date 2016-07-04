namespace Dashing.Tools.SchemaReading {
    using System.Data;

    public interface ISchemaReader {
        Database Read(IDbConnection dbConnection, string databaseName);
    }
}