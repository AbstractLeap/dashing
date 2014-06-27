using System;
using System.Collections.Generic;
using TopHat.Configuration;
using DatabaseSchemaReader.DataSchema;

namespace TopHat.Tools.ModelGeneration
{
    interface IModelGenerator
    {
        IEnumerable<string> GenerateFiles(IConfiguration configuration, DatabaseSchema schema, string domainNamespace);
    }
}
