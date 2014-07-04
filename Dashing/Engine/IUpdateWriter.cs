using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Engine {
    public interface IUpdateWriter : IEntitySqlWriter {
        SqlWriterResult GenerateBulkSql<T>(T updateClass, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}
