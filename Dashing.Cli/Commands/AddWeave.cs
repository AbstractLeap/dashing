namespace Dashing.Cli.Commands {
    using System;
    using System.IO;
    using System.Xml;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;

    public static class AddWeave {
        public static void UseAddWeave(this CommandLineApplication app) {
            app.Command(
                "addweave",
                c => {
                    c.Description = "Executes a function to add weaving to a project";

                    // attempts to weave the assemblies at the specified location
                    var projectFilePath = c.Option("-p|--projectfilepath <path>", "Specify the path to the project file to which weaving should be added", CommandOptionType.SingleValue).IsRequired();
                    var configurationType = c.Option("-c|--configurationtype <typefullname>", "The full name of the configuration type", CommandOptionType.SingleValue).IsRequired();
                    var assemblyExtension = c.Option("-a|--assemblyextension <extension>", "The extension of the assembly (when it has been built, e.g. 'dll', 'exe')", CommandOptionType.SingleValue).IsRequired();

                    c.OnExecute(
                        () => {
                            var projectFileFullPath = Path.GetFullPath(projectFilePath.Value());
                            Program.AssemblySearchDirectories.Insert(0, Path.GetDirectoryName(projectFileFullPath)); // favour user code over dashing code
                            try {
                                ExecuteAddWeave(projectFileFullPath, configurationType, assemblyExtension);
                                return 0;
                            }
                            catch (Exception ex) {
                                Log.Logger.Fatal(ex, "addweave failed unexpectedly");
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteAddWeave(string projectFileFullPath, CommandOption configurationType, CommandOption assemblyExtension) {
            if (!File.Exists(projectFileFullPath)) {
                throw new Exception($"Project {projectFileFullPath} does not exist");
            }

            var xmlDocument = new XmlDocument();
            using (var projectFileStream = File.OpenRead(projectFileFullPath)) {
                xmlDocument.Load(projectFileStream);
            }

            if (!string.Equals("Project", xmlDocument.DocumentElement.LocalName, StringComparison.OrdinalIgnoreCase)) {
                throw new Exception("The project file must have a root element of <Project ...>");
            }

            var propertyGroup = xmlDocument.CreateElement("PropertyGroup");
            var property = xmlDocument.CreateElement("WeaveArguments");
            property.InnerText = $"-p \"$(MSBuildThisFileDirectory)$(OutputPath)$(AssemblyName).{assemblyExtension.Value()}\" -t \"{configurationType.Value()}\"";
            propertyGroup.AppendChild(property);
            var wasInserted = false;
            for (var i = 0; i < xmlDocument.DocumentElement.ChildNodes.Count; i++) {
                var childNode = xmlDocument.DocumentElement.ChildNodes[i];
                if (string.Equals("Import", childNode.LocalName, StringComparison.OrdinalIgnoreCase) && (childNode.Attributes["Project"]
                                                                                                                  ?.Value?.ToUpperInvariant()
                                                                                                                  .Contains("DASHING") ?? false)) {
                    xmlDocument.DocumentElement.InsertBefore(propertyGroup, childNode);
                    wasInserted = true;
                    break;
                }
            }

            if (!wasInserted) {
                xmlDocument.DocumentElement.InsertAfter(propertyGroup, xmlDocument.DocumentElement.LastChild);
            }

            using (var writeProjectStream = File.OpenWrite(projectFileFullPath)) {
                xmlDocument.Save(writeProjectStream);
            }
        }
    }
}