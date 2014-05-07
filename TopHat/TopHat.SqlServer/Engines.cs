using TopHat.SqlServer;

// Dear ReSharper, I know this is a little unusual, but go with it.
// ReSharper disable once CheckNamespace
namespace TopHat {
    public static class Engines {
        public static IEngine SqlServer = new SqlServerEngine();
    }
}