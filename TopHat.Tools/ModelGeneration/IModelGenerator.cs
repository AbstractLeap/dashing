using System;
using System.Collections.Generic;
using TopHat.Configuration;
using DatabaseSchemaReader.DataSchema;

namespace TopHat.Tools.ModelGeneration
{
    interface IModelGenerator
    {
        IDictionary<string, string> GenerateFiles(IEnumerable<IMap> maps, DatabaseSchema schema, string domainNamespace);
    }
}
