using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat
{
    internal class WhereExecuter<T> : IWhereExecute<T>
    {
        private ISession topHat;
        private QueryType queryType;

				public WhereExecuter(ISession topHat, QueryType queryType)
        {
            this.topHat = topHat;
            this.queryType = queryType;
        }

        public void Where(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(predicate));

            ExecuteQuery(query);
        }

        public void Where(string condition)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(condition));

            ExecuteQuery(query);
        }

        public void Where(string condition, params dynamic[] parameters)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(condition, parameters));

            ExecuteQuery(query);
        }

        private void ExecuteQuery(Query<T> query)
        {
            this.topHat.Execute(query);
        }
    }
}