namespace Dashing {
    using System.Collections.Generic;

    public interface IExecuteSelectQueries {
        IEnumerable<T> Query<T>(SelectQuery<T> query);

        Page<T> QueryPaged<T>(SelectQuery<T> query);
    }
}