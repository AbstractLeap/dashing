namespace TopHat.Engine {
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public interface IDapperWrapper {
        int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);

        IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}