namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Data.SqlClient;

    using Dashing.Migration;

    using Xunit;

    public class ConnectionStringManipulatorTests {
        [Theory]
        [InlineData("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;", "Data Source=myServerAddress;User ID=myUsername;Password=myPassword")]
        [InlineData("Server=myServerAddress;Uid=myUsername;Pwd=myPassword;", "Data Source=myServerAddress;User ID=myUsername;Password=myPassword")]
        [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "Data Source=myServerAddress;User ID=myUsername;Password=myPassword")]
        [InlineData("Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;", "Data Source=myServerAddress;Integrated Security=True")]
        [InlineData("Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Initial Catalog=myDataBase;Integrated Security=True;", "Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Integrated Security=True")]
        [InlineData("Data Source=myServerAddress;Integrated Security=SSPI;User ID=myDomain\\myUsername;Password=myPassword;", "Data Source=myServerAddress;Integrated Security=True;User ID=myDomain\\myUsername;Password=myPassword")]
        public void RootConnectionStringWorks(string connectionString, string expectedResult) {
            var connectionStringManipulator = new ConnectionStringManipulator(SqlClientFactory.Instance, connectionString);
            Assert.Equal(expectedResult, connectionStringManipulator.GetRootConnectionString());
        }

        [Theory]
        [InlineData("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")]
        [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
        [InlineData("Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;")]
        [InlineData("Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Initial Catalog=myDataBase;Integrated Security=True;")]
        public void GetDatabaseNameWorks(string connectionString) {
            var connectionStringManipulator = new ConnectionStringManipulator(SqlClientFactory.Instance, connectionString);
            Assert.Equal("myDataBase", connectionStringManipulator.GetDatabaseName());
        }

        [Theory]
        [InlineData("Server=myServerAddress;Uid=myUsername;Pwd=myPassword;")]
        [InlineData(@"Data Source=myServerAddress;Integrated Security=SSPI;User ID=myDomain\myUsername;Password=myPassword;")]
        public void NoDatabaseNameThrows(string connectionString) {
            var connectionStringManipulator = new ConnectionStringManipulator(SqlClientFactory.Instance, connectionString);
            Assert.Throws<NotSupportedException>(() => connectionStringManipulator.GetDatabaseName());
        }
    }
}