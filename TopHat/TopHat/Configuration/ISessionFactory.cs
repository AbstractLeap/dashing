using System.Data;

namespace TopHat.Configuration {
	public interface ISessionFactory {
		ISession Create(IEngine engine, IQueryFactory queryFactory, IDbConnection connection);
		ISession Create(IEngine engine, IQueryFactory queryFactory, IDbConnection connection, IDbTransaction transaction);
	}
}