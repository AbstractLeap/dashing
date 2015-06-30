namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using Newtonsoft.Json;

    public class ConfigurationMapResolver : MarshalByRefObject {
        public void Resolve(ConfigurationMapResolverArgs args) {
            // load the assembly if necessary
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == args.AssemblyFullName);
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(args.AssemblyFilePath);
            if (assembly == null) {
                assembly = Assembly.LoadFile(args.AssemblyFilePath);
            }

            // find any IConfigs, instantiate and return map definitions
            var mapDefinitions = new List<MapDefinition>();
            var configurationTypes =
                assembly.GetTypes().Where(t => typeof(IConfiguration).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.IsPublic);
            if (configurationTypes.Any()) {
                foreach (var configurationType in configurationTypes) {
                    this.InjectConnectionStringIntoConfiguration(
                        assemblyDefinition.MainModule.Types.Single(t => t.FullName == configurationType.FullName));
                    var config = Activator.CreateInstance(configurationType) as IConfiguration;
                    foreach (var map in config.Maps) {
                        mapDefinitions.Add(
                            new MapDefinition {
                                                  AssemblyFullName = map.Type.Assembly.FullName,
                                                  TypeFullName = map.Type.FullName,
                                                  ColumnDefinitions =
                                                      map.OwnedColumns(true)
                                                         .Select(
                                                             c =>
                                                             new ColumnDefinition {
                                                                                      Name = c.Name,
                                                                                      TypeFullName = c.Type.FullName,
                                                                                      Relationship = c.Relationship,
                                                                                      DbName = c.DbName,
                                                                                      DbType = c.DbType,
                                                                                      IsPrimaryKey = c.IsPrimaryKey
                                                                                  })
                                              });
                    }
                }
            }

            args.SerializedConfigurationMapDefinitions = JsonConvert.SerializeObject(mapDefinitions);
        }

        private void InjectConnectionStringIntoConfiguration(TypeDefinition configTypeDefinition) {
            var constructor = configTypeDefinition.Methods.FirstOrDefault(m => m.IsConstructor && !m.HasParameters); // default constructor
            //if (constructor == null) {
            //    this.Log.LogError("Unable to find parameterless constructor on {0}", configTypeDefinition.FullName);
            //}

            var getConnectionStringCall =
                constructor.Body.Instructions.FirstOrDefault(
                    i =>
                    i.OpCode.Code == Code.Call
                    && i.Operand.ToString()
                    == "System.Configuration.ConnectionStringSettingsCollection System.Configuration.ConfigurationManager::get_ConnectionStrings()");
            if (getConnectionStringCall == null) {
                return;
            }

            var connectionStringKey = getConnectionStringCall.Next.Operand.ToString();

            // override readonly property of connectionstrings
            var readOnlyField = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            if (readOnlyField != null) {
                readOnlyField.SetValue(ConfigurationManager.ConnectionStrings, false);
            }

            // remove any existing
            if (ConfigurationManager.ConnectionStrings[connectionStringKey] != null) {
                ConfigurationManager.ConnectionStrings.Remove(connectionStringKey);
            }

            // and add in the one from our ini
            ConfigurationManager.ConnectionStrings.Add(
                new ConnectionStringSettings(
                    connectionStringKey,
                    "Server=(LocalDB)\v11.0; Integrated Security=True; MultipleActiveResultSets=True",
                    "System.Data.SqlClient"));
        }
    }

    public class ConfigurationMapResolverArgs : MarshalByRefObject {
        public string AssemblyFilePath { get; set; }

        public string AssemblyFullName { get; set; }

        public string SerializedConfigurationMapDefinitions { get; set; }
    }

    public class MapDefinition {
        public string AssemblyFullName { get; set; }

        public string TypeFullName { get; set; }

        public IEnumerable<ColumnDefinition> ColumnDefinitions { get; set; }
    }

    public class ColumnDefinition {
        public string Name { get; set; }

        public string TypeFullName { get; set; }

        public RelationshipType Relationship { get; set; }

        public string DbName { get; set; }

        public bool IsPrimaryKey { get; set; }

        public DbType DbType { get; set; }
    }
}