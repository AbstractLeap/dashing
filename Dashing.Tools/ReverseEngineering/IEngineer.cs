namespace Dashing.Tools.ReverseEngineering {
    using System.Collections.Generic;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using DatabaseSchemaReader.DataSchema;

    public interface IEngineer {
        IEnumerable<IMap> ReverseEngineer(DatabaseSchema schema, ISqlDialect sqlDialect, IEnumerable<string> tablesToIgnore);
    }
}