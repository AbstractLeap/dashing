namespace Dashing.Engine.DML {
    using System.Collections.Generic;

    public interface IUpdateWriter {
        SqlWriterResult GenerateSql<T>(IEnumerable<T> entities);
    }
}