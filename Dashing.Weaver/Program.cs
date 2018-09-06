namespace Dashing.Weaver {
    using Dashing.Weaver.Commands;

    using McMaster.Extensions.CommandLineUtils;

    public class Program {
        public static int Main(string[] args) {
            var app = new CommandLineApplication {
                                                     Name = "dashing-weaver",
                                                     Description = "Weaves domain classes in order to support Dashing functionality"
                                                 };
            app.HelpOption(inherited: true);
            app.UseExtractConfigs();
            app.UseWeave();
            return app.Execute(args);
        }
    }
}