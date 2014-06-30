using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TopHat.Tests.Configuration.ReverseEngineer
{
    public class Sandbox
    {
        const string ConnectionString = "Server=localhost;Database=mercury_dev;Uid=root;Password=treatme123;Allow User Variables=true";
        const string ProviderName = "MySql.Data.MySqlClient";

        //[Fact]
        //public void Test()
        //{
        //    var engineer = new TopHat.Configuration.ReverseEngineer.Engineer();
        //    var schemaReader = new DatabaseSchemaReader.DatabaseReader(ConnectionString, DatabaseSchemaReader.DataSchema.SqlType.MySql);
        //    var maps = engineer.ReverseEngineer(schemaReader);
        //}
    }
}
