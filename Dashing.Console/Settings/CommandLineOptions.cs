using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Dashing.Console.Settings {
    class CommandLineOptions {
        [Option('n', HelpText = "Perform a naive migration", Required = false)]
        public bool Naive { get; set; }

        [Option('s', HelpText = "Generate a Migration Script", Required = false, MutuallyExclusiveSet = "ReverseEngineer")]
        public bool Script { get; set; }

        [Option('m', HelpText = "Perform the database migration", Required = false, MutuallyExclusiveSet = "ReverseEngineer")]
        public bool Migration { get; set; }

        [Option('r', HelpText = "Reverse Engineer a Database", Required = false, MutuallyExclusiveSet = "Script,Migration")]
        public bool ReverseEngineer { get; set; }

        [Option('f', HelpText = "This will force a migration, preventing a prompt to override", Required = false)]
        public bool Force { get; set; }

        [Option('p', HelpText = "A matching <projectname>.ini file should be present", Required = true)]
        public string ProjectName { get; set; }

        [Option('l', HelpText = "The location/path where you'd like any generated scripts created", Required = false)]
        public string Location { get; set; }

        public string IniPath { get {
            return this.ProjectName + ".ini";
        } }
    }
}
