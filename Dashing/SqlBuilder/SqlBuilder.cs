using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.SqlBuilder
{
    public enum JoinType
    {
        InnerJoin,
        LeftJoin,
        RightJoin,
        FullOuterJoin
    }

    public class CommandDefinition
    {
        public string Sql { get; set; }

        public object Parameters { get; set; }
    }

    class SqlBuilder
    {
        private readonly ISession session;

        public SqlBuilder(ISession session)
        {
            this.session = session;
        }

        //public ISqlFromDefinition<T> From<T>()
        //{
        //    return new SqlFromDefinition<T>(this.session);
        //}
    }

     /*
     * Select
     * Into
     * From
     * Where
     * Having
     * Group By
     * Order By
     * Paging
     * 
     * With
     * Union
     * Intersect
     * Except
     */
}
