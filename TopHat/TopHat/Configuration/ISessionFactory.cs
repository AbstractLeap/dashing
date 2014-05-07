using System.Data;
using TopHat.SqlWriter;

namespace TopHat.Configuration {
	public interface ISessionFactory {
		ISession Create(ISqlWriter sqlWriter, IQueryFactory queryFactory, IDbConnection connection);
		ISession Create(ISqlWriter sqlWriter, IQueryFactory queryFactory, IDbConnection connection, IDbTransaction transaction);
	}
}