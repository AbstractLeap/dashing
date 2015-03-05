namespace Dashing.Tools.Migration {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public interface IMigrator {
        string GenerateSqlDiff(IEnumerable<IMap> fromMaps, IEnumerable<IMap> toMaps, IAnswerProvider answerProvider, Action<string, object[]> trace, IEnumerable<string> indexesToIgnore, out IEnumerable<string> warnings, out IEnumerable<string> errors);
    }
}