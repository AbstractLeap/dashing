using System.Data;
using TopHat.SqlWriter;

namespace TopHat.Configuration {
	public class DefaultSessionFactory : ISessionFactory {
		public ISession Create(ISqlWriter sqlWriter, IQueryFactory queryFactory, IDbConnection connection) {
			return new Session(sqlWriter, queryFactory, connection);
		}
		public ISession Create(ISqlWriter sqlWriter, IQueryFactory queryFactory, IDbConnection connection, IDbTransaction transaction) {
			return new Session(sqlWriter, queryFactory, connection, transaction);
		}
	}
}