namespace Dashing.IntegrationTests.Setup {
    using System.Reflection;

    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public static class SqlSessionCreatorExtensions {
        public static ISqlDialect GetDialect(this SqlSessionCreator sessionCreator) {
            var engine = (SqlEngine)typeof(SqlSessionCreator)
                                                  .GetField("engine", BindingFlags.Instance | BindingFlags.NonPublic)
                                                  .GetValue(sessionCreator);
            return engine.SqlDialect;
        }
    }
}