namespace Dashing.Weaver.ConfigurationMetadataGeneration {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.CommandLine;
    using Dashing.Configuration;
    using Dashing.Extensions;
    using Dashing.Weaver.Weaving;

    using Newtonsoft.Json;

    public class ConfigurationMetadataGenerator {
        public string GenerateMetadata(IList<string> assemblyPaths, IList<string> configurationTypes) {
            // load all of the assemblies, find the configuration types contained within, new them up, extract the meta data
            var assemblies = LoadAssemblies(assemblyPaths);
            var result = new List<MapDefinition>();
            foreach (var configurationTypeFullName in configurationTypes) {
                var processed = false;
                foreach (var assembly in assemblies) {
                    var configurationType = assembly.GetLoadableTypes().SingleOrDefault(t => t.FullName == configurationTypeFullName);
                    if (configurationType != null) {
                        var configuration = Activator.CreateInstance(configurationType) as IConfiguration;
                        if (configuration == null) {
                            throw new WeaveException($"The type {configurationTypeFullName} does not implement IConfiguration");
                        }

                        foreach (var map in configuration.Maps) {
                            result.Add(
                                new MapDefinition {
                                                      AssemblyPath = map.Type.Assembly().Location,
                                                      TypeFullName = map.Type.FullName,
                                                      IsOwned = map.IsOwned,
                                                      ColumnDefinitions = map
                                                          .Columns.Where(c => !c.Value.IsIgnored)
                                                          .Select(c => c.Value)
                                                          .Select(
                                                              c => new ColumnDefinition {
                                                                                            Name = c.Name,
                                                                                            TypeFullName = c.Type.FullName,
                                                                                            Relationship = c.Relationship,
                                                                                            DbName = c.DbName,
                                                                                            DbType = c.DbType,
                                                                                            IsPrimaryKey = c.IsPrimaryKey,
                                                                                            RelatedTypePrimarykeyName =
                                                                                                c.Relationship == RelationshipType.ManyToOne
                                                                                                    ? c.ParentMap.PrimaryKey.Name
                                                                                                    : (c.Relationship == RelationshipType.OneToOne
                                                                                                           ? c.OppositeColumn.Map.PrimaryKey.Name
                                                                                                           : null),
                                                                                            ShouldWeavingInitialiseListInConstructor = c.ShouldWeavingInitialiseListInConstructor
                                                                                        })
                                                          .ToList()
                                                  });
                        }

                        processed = true;
                        break;
                    }
                }

                if (!processed) {
                    throw new WeaveException($"Unable to locate {configurationTypeFullName} in any of the assemblies");
                }
            }

            return JsonConvert.SerializeObject(result);
        }

        private static IList<Assembly> LoadAssemblies(IList<string> assemblyPaths) {
            var assemblies = new List<Assembly>();
            foreach (var assemblyPath in assemblyPaths) {
                if (!File.Exists(assemblyPath)) {
                    throw new WeaveException($"Could not find file at {assemblyPath}");
                }

                assemblies.Add(AssemblyContext.LoadFile(assemblyPath));
            }

            return assemblies;
        }
    }
}