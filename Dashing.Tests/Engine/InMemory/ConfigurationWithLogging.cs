namespace Dashing.Tests.Engine.InMemory {
    using Dashing.Configuration;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    public class ConfigurationWithLogging : BaseConfiguration {
        public ConfigurationWithLogging()
            : base(
                new PolyLoggerBuilder().AddConsole(
                                           new Poly.Logging.Models.ConsoleConfiguration {
                                                                                            LogToDebug = true
                                                                                        })
                                       .AddTextFile(
                                           new Poly.Logging.TextFile.Models.TextFileConfiguration {
                                                                                                      DirectoryPath = "c:\\TestLogs\\"
                                                                                                  })
                                       .Build()) {
            this.AddNamespaceOf<Post>();
            this.Setup<Post>()
                .Property(p => p.DoNotMap)
                .Ignore();
        }
    }
}