namespace Dashing.Console.Settings {
    using System.IO;

    using CommandLine;

    internal class CommandLineOptions {
        private string configPath;

        private string projectName;

        [Option('n', HelpText = "Use the naïve migration strategy", Required = false)]
        public bool Naive { get; set; }

        [Option('s', HelpText = "Output the migration Script", Required = false, MutuallyExclusiveSet = "ReverseEngineer,Weave")]
        public bool Script { get; set; }

        [Option('m', HelpText = "Perform the database migration", Required = false, MutuallyExclusiveSet = "ReverseEngineer,Weave")]
        public bool Migration { get; set; }

        [Option('e', HelpText = "Seed the database with default values", Required = false, MutuallyExclusiveSet = "ReverseEngineer,Weave")]
        public bool Seed { get; set; }

        [Option('r', HelpText = "Reverse engineer a database", Required = false, MutuallyExclusiveSet = "Script,Migration,Weave")]
        public bool ReverseEngineer { get; set; }

        [Option('w', HelpText = "Weave the dlls in a directory", Required = false, MutuallyExclusiveSet = "Script,Migration,ReverseEngineer")]
        public bool Weave { get; set; }

        [Option('d', HelpText = "The directory that contains the dlls to weave", Required = false)]
        public string WeaveDir { get; set; }

        [Option('b', HelpText = "Launch debugger during weaving")]
        public bool LaunchDebugger { get; set; }

        [Option('i', HelpText = "Ignore PEVerify Errors on Weaving")]
        public bool IgnorePeVerify { get; set; }

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
                this.configPath = Path.GetFullPath(value);
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