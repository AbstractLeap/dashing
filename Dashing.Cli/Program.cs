namespace Dashing.Cli {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Cli.Commands;
    using Dashing.CommandLine;

    using McMaster.Extensions.CommandLineUtils;

#if !COREFX
    using System.Configuration;
#endif

    public class Program {
#if COREFX
        internal static readonly IList<string> AssemblySearchDirectories = new List<string>();
#else
        internal static readonly IList<string> AssemblySearchDirectories = (ConfigurationManager.AppSettings["AssemblySearchPaths"]
                                                                                               ?.Split(';') ?? Enumerable.Empty<string>()).ToList();
#endif

        public static int Main(string[] args) {
            AssemblyResolution.Configure(AssemblySearchDirectories); // we have to configure the assembly resolution on it's own in this method as the ExecuteApplication needs it
            return ExecuteApplication(args);
        }

        private static int ExecuteApplication(string[] args) {
            var app = new CommandLineApplication {
                                                     Name = "dash",
                                                     Description = "Provides functionality to migrate databases"
                                                 };
            app.ConfigureLogging();
            app.HelpOption(inherited: true);
            app.UseScript();
            app.UseMigrate();
            app.UseSeed();
            app.UseAddWeave();
            return app.Execute(args);
        }
    }
}