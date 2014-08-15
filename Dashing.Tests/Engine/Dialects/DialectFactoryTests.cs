namespace Dashing.Tests.Engine.Dialects {
    using System.Configuration;

    using Dashing.Engine.Dialects;

    using Xunit;

    public class DialectFactoryTests {
        [Fact]
        public void MySqlProviderGeneratesMySqlDialect() {
            Assert.IsType<MySqlDialect>(new DialectFactory().Create(new ConnectionStringSettings("Default", "mysql", "MySql.Data.MySqlClient")));
        }

        [Fact]
        public void SqlServerDefaultsTo2012() {
            // i.e. they'll get an error with they attempt to take or skip and then they'll need to modify the connection string
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings("Default", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "System.Data.SqlClient")));
        }

        [Fact]
        public void SqlServer2012Dialect() {
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings(
                        "Default",
                        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=Latest;",
                        "System.Data.SqlClient")));
            Assert.IsType<SqlServer2012Dialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings(
                        "Default",
                        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2012;",
                        "System.Data.SqlClient")));
        }

        [Fact]
        public void OldSqlDialect() {
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings(
                        "Default",
                        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2000;",
                        "System.Data.SqlClient")));
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings(
                        "Default",
                        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2005;",
                        "System.Data.SqlClient")));
            Assert.IsType<SqlServerDialect>(
                new DialectFactory().Create(
                    new ConnectionStringSettings(
                        "Default",
                        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;Type System Version=SQL Server 2008;",
                        "System.Data.SqlClient")));
        }
    }
}