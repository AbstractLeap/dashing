namespace Dashing.IntegrationTests.Setup {
    using System.Reflection;

    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public static class SqlSessionCreatorExtensions {
        public static ISqlDialect GetDialect(this SqlDatabase sessionCreator) {
            var engine = (SqlEngine)typeof(SqlDatabase)
                                                  .GetField("engine", BindingFlags.Instance | BindingFlags.NonPublic)
                                                  .GetValue(sessionCreator);
            return engine.SqlDialect;
        }
    }
}