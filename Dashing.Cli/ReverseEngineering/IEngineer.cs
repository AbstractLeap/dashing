namespace Dashing.ReverseEngineering {
    using System.Collections.Generic;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.SchemaReading;

    public interface IEngineer {
        IEnumerable<IMap> ReverseEngineer(
            Database schema,
            ISqlDialect sqlDialect,
            IEnumerable<string> tablesToIgnore,
            IAnswerProvider answerProvider,
            bool fixOneToOnes);
    }
}