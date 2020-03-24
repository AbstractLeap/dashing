namespace Dashing.Console.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using Newtonsoft.Json;

    public class ConfigurationMapResolver : MarshalByRefObject {
        public void Resolve(ConfigurationMapResolverArgs args) {
            // assembly resolution
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) => {
                var assemblyName = new AssemblyName(eventArgs.Name);

                // look in app domain
                var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    return loaded;
                }

                // we couldn't find it, look on disk
                var path = Path.GetDirectoryName(args.AssemblyFilePath) + @"\" + assemblyName.Name + ".dll";
                if (File.Exists(path)) {
                    var assemblyData = File.ReadAllBytes(path);
                    return Assembly.Load(assemblyData);
                }

                return null;
            };

            // load the assembly if necessary
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.AssemblyFullName);
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(args.AssemblyFilePath, new ReaderParameters { InMemory = true });
            if (assembly == null) {
                assembly = Assembly.LoadFile(args.AssemblyFilePath);
            }

            // find any IConfigs, instantiate and return map definitions
            var mapDefinitions = new List<MapDefinition>();
            var configurationTypes =
                assembly.GetLoadableTypes()
                        .Where(
                            t =>
                            typeof(IConfiguration).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract
                            && t.CustomAttributes.All(a => a.AttributeType != typeof(DoNotWeaveAttribute)));
            if (configurationTypes.Any()) {
                foreach (var configurationType in configurationTypes) {
                    TypeDefinition configTypeDef;
                    if (configurationType.FullName.Contains("+")) {
                        var types = configurationType.FullName.Split('+');
                        configTypeDef = assemblyDefinition.MainModule.Types.Single(t => t.FullName == types.First());
                        for (var i = 1; i < types.Length; i++) {
                            configTypeDef = configTypeDef.NestedTypes.Single(t => t.Name == types.ElementAt(i));
                        }
                    }
                    else {
                        configTypeDef = assemblyDefinition.MainModule.Types.Single(t => t.FullName == configurationType.FullName);
                    }

                    this.InjectConnectionStringIntoConfiguration(configTypeDef);
                    var config = Activator.CreateInstance(configurationType) as IConfiguration;
                    foreach (var map in config.Maps) {
                        mapDefinitions.Add(
                            new MapDefinition {
                                                  AssemblyFullName = map.Type.Assembly.FullName,
                                                  TypeFullName = map.Type.FullName,
                                                  ColumnDefinitions =
                                                      map.Columns.Where(c => !c.Value.IsIgnored)
                                                         .Select(c => c.Value)
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