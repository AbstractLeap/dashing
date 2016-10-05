namespace Dashing.Tests.Engine.Dialects {
    using Dashing.Engine.Dialects;

    using Xunit;

    public class DialectFactoryTests {
        [Fact]
        public void MySqlProviderGeneratesMySqlDialect() {
            Assert.IsType<MySqlDialect>(new DialectFactory().Create("MySql.Data.MySqlClient", "mysql"));
        }

        [Fact]
        public void SqlServerDefaultsTo2012() {
            // i.e. they'll get an error with they attempt to take or skip and then they'll need to modify the connection string
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create("System.Data.SqlClient", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"));
        }

        [Fact]
        public void SqlServer2012Dialect() {
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create("System.Data.SqlClient", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=Latest;"));
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create("System.Data.SqlClient", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2012;"));
        }

        [Fact]
        public void OldSqlDialect() {
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create("System.Data.SqlClient", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2000;"));
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create("System.Data.SqlClient","Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2005;"));
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create("System.Data.SqlClient","Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2008;"));
        }
    }
}