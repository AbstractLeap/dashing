namespace Dashing.Tools.ModelGeneration {
    using System.Collections.Generic;

    using Dashing.Configuration;

    using DatabaseSchemaReader.DataSchema;

    internal interface IModelGenerator {
        IDictionary<string, string> GenerateFiles(IEnumerable<IMap> maps, DatabaseSchema schema, string domainNamespace);
    }
}