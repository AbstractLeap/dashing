namespace Dashing.ModelGeneration {
    using System.Collections.Generic;

    using Dashing.Configuration;
    using Dashing.SchemaReading;

    internal interface IModelGenerator {
        IDictionary<string, string> GenerateFiles(IEnumerable<IMap> maps, Database schema, string domainNamespace, IAnswerProvider answerProvider);
    }
}