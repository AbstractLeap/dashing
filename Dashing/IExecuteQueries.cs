namespace Dashing {
    using System.Collections.Generic;

    public interface IExecuteQueries {
        IEnumerable<T> Query<T>(SelectQuery<T> query);
    }
}