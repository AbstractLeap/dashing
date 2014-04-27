using Moq;
using System.Data;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat.Tests.QueryWriter
{
    public class BaseQueryWriterTest
    {
        protected Mock<IDbConnection> conn;
        protected Mock<IDbTransaction> tran;
        protected Mock<ISqlWriter> sql;
        protected Mock<IConfiguration> config;

        public BaseQueryWriterTest()
        {
            conn = new Mock<IDbConnection>();
            tran = new Mock<IDbTransaction>();
            sql = new Mock<ISqlWriter>();
            config = new Mock<IConfiguration>();
        }

        protected ITopHat GetTopHat()
        {
            return new TopHat(conn.Object, tran.Object, config.Object, sql.Object);
        }
    }
}