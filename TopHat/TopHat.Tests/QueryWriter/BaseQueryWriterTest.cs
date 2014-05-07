using System;
using System.Data;
using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using TopHat.Configuration;
using TopHat.SqlWriter;
using TopHat.Tests.TestDomain;

namespace TopHat.Tests.QueryWriter {
	public class BaseQueryWriterTest : IDisposable {
		protected readonly Mock<IDbConnection> Connection;
		protected readonly Mock<IDbTransaction> Transaction;
		protected readonly Mock<ISqlWriter> SqlWriter;
		protected readonly Mock<IQueryFactory> QueryFactory;
		private readonly IDisposable _shimsContext;

		public BaseQueryWriterTest() {
			Connection = new Mock<IDbConnection>(MockBehavior.Strict);
			Transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
			SqlWriter = new Mock<ISqlWriter>(MockBehavior.Strict);
			QueryFactory = new Mock<IQueryFactory>(MockBehavior.Strict);
			_shimsContext = ShimsContext.Create();
		}

		protected ISession GetTopHat() {
			//Dapper.Fakes.ShimSqlMapper.ExecuteIDbConnectionStringObjectIDbTransactionNullableOfInt32NullableOfCommandType = (connection, SqlWriter, parameters, transaction, timeout, type) => 1;
			
			var session = new Session(SqlWriter.Object, QueryFactory.Object, Connection.Object, Transaction.Object);
			QueryFactory.Setup(m => m.Select<Post>(It.IsAny<ISession>())).Returns(new QueryWriter<Post>(session, false));
			return session;
		}

		public void Dispose() {
			_shimsContext.Dispose();
		}
	}
}