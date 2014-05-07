namespace TopHat.Configuration {
	public interface IQueryFactory {
		ISelect<T> Select<T>(ISession session);
	}
}