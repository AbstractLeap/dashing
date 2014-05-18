namespace TopHat.Tests {
  using System;
  using System.Data;

  using Microsoft.QualityTools.Testing.Fakes;

  using Moq;

  public class BaseQueryWriterTest : IDisposable {
    protected readonly Mock<IDbConnection> Connection;

    protected readonly Mock<IDbTransaction> Transaction;

    private readonly IDisposable _shimsContext;

    public BaseQueryWriterTest() {
      this.Connection = new Mock<IDbConnection>(MockBehavior.Strict);
      this.Transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
      this._shimsContext = ShimsContext.Create();
    }

    protected ISession GetTopHat() {
      // Dapper.Fakes.ShimSqlMapper.ExecuteIDbConnectionStringObjectIDbTransactionNullableOfInt32NullableOfCommandType = (connection, SqlWriter, parameters, transaction, timeout, type) => 1;
      var session = new Session(Engines.SqlServer, this.Connection.Object, this.Transaction.Object);
      return session;
    }

    public void Dispose() {
      this._shimsContext.Dispose();
    }
  }
}