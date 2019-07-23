namespace Dashing.Engine.DML {
    using System.Collections.Generic;

    public interface IDeleteWriter {
        SqlWriterResult GenerateSql<T>(IEnumerable<T> entities);
    }
}