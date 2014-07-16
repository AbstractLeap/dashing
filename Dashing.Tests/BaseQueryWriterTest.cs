namespace Dashing.Tests {
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Engine;

    using Moq;

    public class BaseQueryWriterTest {
        protected readonly Mock<IDbConnection> Connection;

        protected readonly Mock<IDbTransaction> Transaction;

        protected readonly Mock<IGeneratedCodeManager> CodeManager;

        protected readonly Mock<IEngine> MockEngine;

        public BaseQueryWriterTest() {
            this.Connection = new Mock<IDbConnection>(MockBehavior.Strict);
            this.Connection.Setup(c => c.State).Returns(ConnectionState.Open);
            this.Transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            this.CodeManager = new Mock<IGeneratedCodeManager>();
            this.MockEngine = new Mock<IEngine>();
        }

        protected ISession GetDashing() {
            return new Session(this.MockEngine.Object, this.Connection.Object, this.Transaction.Object);
        }
    }
}