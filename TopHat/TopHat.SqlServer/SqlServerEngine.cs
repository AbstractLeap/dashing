namespace TopHat.SqlServer {
	public class SqlServerEngine : EngineBase {
		public SqlServerEngine() : base(new SqlServerConnectionFactory(), new SqlServerSqlWriter()) {}
	}
}