namespace Dashing.Tools.ReverseEngineering {
    using System.Collections.Generic;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.SchemaReading;

    public interface IEngineer {
        IEnumerable<IMap> ReverseEngineer(
            Database schema,
            ISqlDialect sqlDialect,
            IEnumerable<string> tablesToIgnore,
            IAnswerProvider answerProvider,
            bool fixOneToOnes);
    }
}