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
        private ITopHat topHat;
        private QueryType queryType;

        public WhereExecuter(ITopHat topHat, QueryType queryType)
        {
            this.topHat = topHat;
            this.queryType = queryType;
        }

        public void Where(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(predicate));

            this.topHat.SqlWriter.Execute(query);
        }

        public void Where(string condition)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(condition));

            this.topHat.SqlWriter.Execute(query);
        }

        public void Where(string condition, params dynamic[] parameters)
        {
            var query = new Query<T> { QueryType = this.queryType };
            query.WhereClauses.Add(new WhereClause<T>(condition, parameters));

            this.topHat.SqlWriter.Execute(query);
        }
    }
}