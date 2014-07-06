namespace Dashing.Tests.Configuration.ReverseEngineer {
    public class Sandbox {
        private const string ConnectionString = "Server=localhost;Database=mercury_dev;Uid=root;Password=treatme123;Allow User Variables=true";

        private const string ProviderName = "MySql.Data.MySqlClient";

        ////[Fact]
        ////public void Test()
        ////{
        ////    var engineer = new Dashing.Configuration.ReverseEngineer.Engineer();
        ////    var schemaReader = new DatabaseSchemaReader.DatabaseReader(ConnectionString, DatabaseSchemaReader.DataSchema.SqlType.MySql);
        ////    var maps = engineer.ReverseEngineer(schemaReader);
        ////}
    }
}