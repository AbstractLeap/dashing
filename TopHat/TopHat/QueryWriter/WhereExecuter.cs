using System;
using System.Linq.Expressions;

namespace TopHat.QueryWriter {
	internal class WhereExecuter<T> : IWhereExecute<T> {
		private readonly ISession topHat;
		private readonly QueryType queryType;

		public WhereExecuter(ISession topHat, QueryType queryType) {
			this.topHat = topHat;
			this.queryType = queryType;
		}

		public void Where(Expression<Func<T, bool>> predicate) {
			var query = new Query<T> {QueryType = queryType};
			query.WhereClauses.Add(new WhereClause<T>(predicate));

			ExecuteQuery(query);
		}

		public void Where(string condition) {
			var query = new Query<T> {QueryType = queryType};
			query.WhereClauses.Add(new WhereClause<T>(condition));

			ExecuteQuery(query);
		}

		public void Where(string condition, params dynamic[] parameters) {
			var query = new Query<T> {QueryType = queryType};
			query.WhereClauses.Add(new WhereClause<T>(condition, parameters));

			ExecuteQuery(query);
		}

		private void ExecuteQuery(Query<T> query) {
			topHat.Execute(query);
		}
	}
}