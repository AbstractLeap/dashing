namespace TopHat.Tests {
    using System;
    using System.Data;

    using Microsoft.QualityTools.Testing.Fakes;

    using Moq;

    using TopHat.CodeGeneration;
    using TopHat.Configuration;
    using TopHat.Engine;

    public class BaseQueryWriterTest : IDisposable {
        protected readonly Mock<IDbConnection> Connection;

        protected readonly Mock<IDbTransaction> Transaction;

        protected readonly Mock<IGeneratedCodeManager> CodeManager;

        private readonly IDisposable shimsContext;

        private readonly Mock<IConfiguration> config;

        public BaseQueryWriterTest() {
            this.Connection = new Mock<IDbConnection>(MockBehavior.Strict);
            this.Connection.Setup(c => c.State).Returns(ConnectionState.Open);
            this.Transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            this.CodeManager = new Mock<IGeneratedCodeManager>();
            this.shimsContext = ShimsContext.Create();
            this.config = new Mock<IConfiguration>();
        }

        protected ISession GetTopHat() {
            // Dapper.Fakes.ShimSqlMapper.ExecuteIDbConnectionStringObjectIDbTransactionNullableOfInt32NullableOfCommandType = (connection, SqlWriter, parameters, transaction, timeout, type) => 1;
            var session = new Session(this.Connection.Object, this.config.Object, this.Transaction.Object);
            return session;
        }

        public void Dispose() {
            this.shimsContext.Dispose();
        }
    }
}