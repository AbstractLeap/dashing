namespace Dashing.Cli.Commands {
    using System;
    using System.IO;
    using System.Xml;

    using Dashing.CommandLine;

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
                            c.EnableLogging();
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

            if (xmlDocument.DocumentElement == null || !string.Equals("Project", xmlDocument.DocumentElement.LocalName, StringComparison.OrdinalIgnoreCase)) {
                throw new Exception("The project file must have a root element of <Project ...>");
            }

            // if it's the new format we need to split out the props and targets so that $(OutputPath) can work
            if (xmlDocument.DocumentElement.HasAttribute("Sdk") && xmlDocument.DocumentElement.GetAttribute("Sdk")
                                                                              .StartsWith("Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase)) {
                var sdk = xmlDocument.DocumentElement.GetAttribute("Sdk");
                xmlDocument.DocumentElement.RemoveAttribute("Sdk");
                var propsImport = xmlDocument.CreateElement("Import");
                propsImport.SetAttribute("Sdk", sdk);
                propsImport.SetAttribute("Project", "Sdk.props");
                xmlDocument.DocumentElement.PrependChild(propsImport);
                var targetsImport = xmlDocument.CreateElement("Import");
                targetsImport.SetAttribute("Sdk", sdk);
                targetsImport.SetAttribute("Project", "Sdk.targets");
                xmlDocument.DocumentElement.AppendChild(targetsImport);
            }

            // create the property group
            var propertyGroup = xmlDocument.CreateElement("PropertyGroup", xmlDocument.DocumentElement.NamespaceURI);
            var property = xmlDocument.CreateElement("WeaveArguments", xmlDocument.DocumentElement.NamespaceURI);
            property.InnerText = $"-p \"$(MSBuildThisFileDirectory)$(OutputPath)$(AssemblyName).{assemblyExtension.Value()}\" -t \"{configurationType.Value()}\"";
            propertyGroup.AppendChild(property);

            // for .Net Framwork nuget inserts an <Import ... > to add in the Weaver target, we need to insert before that
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
                xmlDocument.DocumentElement.AppendChild(propertyGroup);
            }

            using (var writeProjectStream = File.OpenWrite(projectFileFullPath)) {
                xmlDocument.Save(writeProjectStream);
            }
        }
    }
}