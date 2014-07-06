namespace Dashing.Tools.ReverseEngineering {
    using System.Collections.Generic;

    using Dashing.Configuration;

    using DatabaseSchemaReader.DataSchema;

    public interface IEngineer {
        IEnumerable<IMap> ReverseEngineer(DatabaseSchema schema);
    }
}