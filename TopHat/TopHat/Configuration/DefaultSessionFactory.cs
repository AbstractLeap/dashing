using System.Data;

namespace TopHat.Configuration {
	public class DefaultSessionFactory : ISessionFactory {
		public ISession Create(IEngine engine, IQueryFactory queryFactory, IDbConnection connection) {
			return new Session(engine, queryFactory, connection);
		}

		public ISession Create(IEngine engine, IQueryFactory queryFactory, IDbConnection connection, IDbTransaction transaction) {
			return new Session(engine, queryFactory, connection, transaction);
		}
	}
}