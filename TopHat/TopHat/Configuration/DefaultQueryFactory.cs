namespace TopHat.Configuration {
	public class DefaultQueryFactory : IQueryFactory {
		public ISelect<T> Select<T>(ISession session) {
			return new QueryWriter<T>(session, false);
		}
	}
}