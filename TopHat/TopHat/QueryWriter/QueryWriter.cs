using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TopHat {
	public class QueryWriter<T> : ISelect<T> {
		private ISession topHat;
		private ISelect<T> query;

		public QueryWriter(ISession topHat, bool tracked) {
			this.topHat = topHat;
			this.query = new SelectQuery<T> { Tracked = tracked, QueryType = QueryType.Select };
		}

		public IFetch<T> Select(Expression<Func<T, dynamic>> selectExpression) {
			throw new NotImplementedException();
		}

		public ISelect<T> IncludeAll() {
			this.query.FetchAllProperties = true;
			return this;
		}

		public ISelect<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
			this.query.Includes.Add(includeExpression);
			return this;
		}

		public ISelect<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
			this.query.Excludes.Add(excludeExpression);
			return this;
		}

		public IThenFetch<T, TFetch> Fetch<TFetch>(Expression<Func<T, TFetch>> relatedObjectSelector) {
			throw new NotImplementedException();
		}

		public IThenFetch<T, TFetch> FetchMany<TFetch>(Expression<Func<T, IEnumerable<TFetch>>> relatedObjectSelector) {
			throw new NotImplementedException();
		}

		public IWhere<T> Where(Expression<Func<T, bool>> predicate) {
			this.query.WhereClauses.Add(new WhereClause<T>(predicate));
			return this;
		}

		public IWhere<T> Where(string condition) {
			this.query.WhereClauses.Add(new WhereClause<T>(condition));
			return this;
		}

		public IWhere<T> Where(string condition, params dynamic[] parameters) {
			this.query.WhereClauses.Add(new WhereClause<T>(condition, parameters));
			return this;
		}

		public IOrder<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
			this.query.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Ascending));
			return this;
		}

		public IOrder<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
			this.query.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Descending));
			return this;
		}

		public IOrder<T> OrderBy(string condition) {
			this.query.OrderClauses.Enqueue(new OrderClause<T>(condition, ListSortDirection.Ascending));
			return this;
		}

		public IOrder<T> OrderByDescending(string condition) {
			this.query.OrderClauses.Enqueue(new OrderClause<T>(condition, ListSortDirection.Descending));
			return this;
		}

		public IQuery<T> ForUpdate() {
			this.query.ForUpdate = true;
			return this;
		}

		public IQuery<T> Skip(int skip) {
			this.query.Skip = skip;
			return this;
		}

		public IQuery<T> Take(int take) {
			this.query.Take = take;
			return this;
		}

		public Task<IEnumerable<T>> Async() {
			throw new NotImplementedException();
		}

		public Query<T> Query {
			get { return this.query; }
		}

		public IEnumerator<T> GetEnumerator() {
			if (this.query.Fetches.Count == 0) {
				if (!this.query.Tracked) {
					return this.topHat.Query<T>(query).GetEnumerator();
				}
				else {
					throw new NotImplementedException();
				}
			}
			else {
				throw new NotImplementedException();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}
	}

}