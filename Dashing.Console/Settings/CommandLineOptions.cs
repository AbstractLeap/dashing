namespace Dashing.Console.Settings {
    using System.IO;

    using CommandLine;

    internal class CommandLineOptions {
        private string configPath;

        private string projectName;

        [Option('n', HelpText = "Use the naïve migration strategy", Required = false)]
        public bool Naive { get; set; }

        [Option('s', HelpText = "Output the migration Script", Required = false, MutuallyExclusiveSet = "ReverseEngineer")]
        public bool Script { get; set; }

        [Option('m', HelpText = "Perform the database migration", Required = false, MutuallyExclusiveSet = "ReverseEngineer")]
        public bool Migration { get; set; }

        [Option('e', HelpText = "Seed the database with default values", Required = false, MutuallyExclusiveSet = "ReverseEngineer")]
        public bool Seed { get; set; }

        [Option('r', HelpText = "Reverse engineer a database", Required = false, MutuallyExclusiveSet = "Script,Migration")]
        public bool ReverseEngineer { get; set; }

        [Option('v', HelpText = "Verbose", Required = false)]
        public bool Verbose { get; set; }

        // this doesnt seem to do anything?
        ////[Option('f', HelpText = "This will force a migration, preventing a prompt to override", Required = false)]
        ////public bool Force { get; set; }

        [Option('c', HelpText = "Specify the configuration file (e.g. -c config.ini)", Required = false)]
        public string ConfigPath {
            get {
                return this.configPath;
            }

            set {
                this.configPath = value;
                this.projectName = Path.GetFileNameWithoutExtension(value);
            }
        }

        [Option('p', HelpText = "Specify the project name (equivalent to -c projectname.ini)", Required = false)]
        public string ProjectName {
            get {
                return this.projectName;
            }

            set {
                this.projectName = value;
                this.configPath = value + ".ini";
            }
        }

        [Option('l', HelpText = "Specify the location to output generated files", Required = false)]
        public string Location { get; set; }
    }
}