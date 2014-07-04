using System;
using System.Collections.Generic;
using Dashing.Configuration;
using DatabaseSchemaReader.DataSchema;

namespace Dashing.Tools.ModelGeneration
{
    interface IModelGenerator
    {
        IDictionary<string, string> GenerateFiles(IEnumerable<IMap> maps, DatabaseSchema schema, string domainNamespace);
    }
}
