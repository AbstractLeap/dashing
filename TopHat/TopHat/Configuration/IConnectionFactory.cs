using System.Data;

namespace TopHat.Configuration {
	public interface IConnectionFactory {
		IDbConnection Open(string connectionString);
	}
}