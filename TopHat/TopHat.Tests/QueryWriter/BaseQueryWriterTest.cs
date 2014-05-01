using Dapper;
using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using System;
using System.Data;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat.Tests.QueryWriter
{
    public class BaseQueryWriterTest : IDisposable
    {
        protected Mock<IDbConnection> conn;
        protected Mock<IDbTransaction> tran;
        protected Mock<ISqlWriter> sql;
        protected Mock<IConfiguration> config;
        private IDisposable shimsContext;

        public BaseQueryWriterTest()
        {
            this.conn = new Mock<IDbConnection>();
            this.tran = new Mock<IDbTransaction>();
            this.sql = new Mock<ISqlWriter>();
            this.config = new Mock<IConfiguration>();
            this.shimsContext = ShimsContext.Create();
        }

        protected ITopHat GetTopHat()
        {
            config.Setup(c => c.GetSqlWriter()).Returns(this.sql.Object);
            Dapper.Fakes.ShimSqlMapper.ExecuteIDbConnectionStringObjectIDbTransactionNullableOfInt32NullableOfCommandType = (connection, sql, parameters, transaction, timeout, type) => 1;
            return new TopHat(config.Object, conn.Object, tran.Object);
        }

        public void Dispose()
        {
            this.shimsContext.Dispose();
        }
    }
}