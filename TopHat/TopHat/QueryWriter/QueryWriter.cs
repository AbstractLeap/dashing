using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat
{
    public class QueryWriter<T> : ISelect<T>
    {
        private Configuration.IConfiguration configuration;
        private SqlWriter.ISqlWriter sqlWriter;
        private Query<T> query;

        public QueryWriter(Configuration.IConfiguration configuration, SqlWriter.ISqlWriter sqlWriter, bool tracked)
        {
            this.configuration = configuration;
            this.sqlWriter = sqlWriter;
            this.query = new Query<T> { Tracked = tracked };
        }

        public IFetch<T> Select(System.Linq.Expressions.Expression<Func<T, dynamic>> selectExpression)
        {
            throw new NotImplementedException();
        }

        public ISelect<T> IncludeAll()
        {
            throw new NotImplementedException();
        }

        public ISelect<T> Include<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> includeExpression)
        {
            throw new NotImplementedException();
        }

        public ISelect<T> Exclude<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> excludeExpression)
        {
            throw new NotImplementedException();
        }

        public IThenFetch<T, TFetch> Fetch<TFetch>(System.Linq.Expressions.Expression<Func<T, TFetch>> relatedObjectSelector)
        {
            throw new NotImplementedException();
        }

        public IThenFetch<T, TFetch> FetchMany<TFetch>(System.Linq.Expressions.Expression<Func<T, IEnumerable<TFetch>>> relatedObjectSelector)
        {
            throw new NotImplementedException();
        }

        public IWhere<T> Where(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            this.query.WhereClauses.Add(new WhereClause<T>(predicate));
            return this;
        }

        public IWhere<T> Where(string condition)
        {
            this.query.WhereClauses.Add(new WhereClause<T>(condition));
            return this;
        }

        public IWhere<T> Where(string condition, params dynamic[] parameters)
        {
            this.query.WhereClauses.Add(new WhereClause<T>(condition, parameters));
            return this;
        }

        public IOrder<T> OrderBy<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> keySelector)
        {
            this.query.OrderClauses.Enqueue(new OrderClause<T>(keySelector, System.ComponentModel.ListSortDirection.Ascending));
            return this;
        }

        public IOrder<T> OrderByDescending<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> keySelector)
        {
            this.query.OrderClauses.Enqueue(new OrderClause<T>(keySelector, System.ComponentModel.ListSortDirection.Descending));
            return this;
        }

        public IOrder<T> OrderBy(string condition)
        {
            this.query.OrderClauses.Enqueue(new OrderClause<T>(condition, System.ComponentModel.ListSortDirection.Ascending));
            return this;
        }

        public IOrder<T> OrderByDescending(string condition)
        {
            this.query.OrderClauses.Enqueue(new OrderClause<T>(condition, System.ComponentModel.ListSortDirection.Descending));
            return this;
        }

        public IQuery<T> ForUpdate()
        {
            this.query.ForUpdate = true;
            return this;
        }

        public IQuery<T> Skip(int skip)
        {
            this.query.Skip = skip;
            return this;
        }

        public IQuery<T> Take(int take)
        {
            this.query.Take = take;
            return this;
        }

        public Task<IEnumerable<T>> Async()
        {
            throw new NotImplementedException();
        }

        public Query<T> Query
        {
            get { return this.query; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}