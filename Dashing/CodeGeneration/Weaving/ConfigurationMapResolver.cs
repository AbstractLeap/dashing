namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;

    using Newtonsoft.Json;

    public class ConfigurationMapResolver
    {
        public void Resolve(string assemblyFilePath)
        {
            
            //var assembly = PlatformServices.Default.AssemblyLoadContextAccessor.Default.LoadFile(assemblyFilePath);
            var configTypes =
                assembly.GetLoadableTypes()
                        .Where(
                            t =>
                                typeof(IConfiguration).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract
                                && t.GetTypeInfo().CustomAttributes.All(a => a.AttributeType != typeof(DoNotWeaveAttribute)));
            if (configTypes.Any())
            {
                // we return a dictionary where the key is the FullName of the IConfig type
                // and the value is another dictionary where that key is the FullName of the Map Type
                var results = new Dictionary<string, IDictionary<string, IEnumerable<ColumnDefinition>>>();
                foreach (var configType in configTypes)
                {
                    var defaultConstructor = configType.GetConstructor(Type.EmptyTypes);
                    IConfiguration config;
                    if (defaultConstructor != null)
                    {
                        config = (IConfiguration)Activator.CreateInstance(configType);
                    }
                    else
                    {
                        var stringConstructor = configType.GetConstructor(new[] { typeof(string) });
                        if (stringConstructor != null)
                        {
                            config = (IConfiguration)Activator.CreateInstance(configType, "Data Source=.");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Unable to instantiate {0}. IConfigurations need either a parameterless constructor or one that takes a string");
                            return;
                        }
                    }

                    results.Add(
                        configType.FullName,
                        config.Maps.ToDictionary(
                            m => m.Type.FullName,
                            m =>
                                m.Columns.Where(c => !c.Value.IsIgnored)
                                 .Select(c => c.Value)
                                 .Select(
                                     c =>
                                         new ColumnDefinition
                                         {
                                             Name = c.Name,
                                             TypeFullName = c.Type.FullName,
                                             Relationship = c.Relationship,
                                             DbName = c.DbName,
                                             DbType = c.DbType,
                                             IsPrimaryKey = c.IsPrimaryKey,
                                             RelatedTypePrimarykeyName = c.Relationship == RelationshipType.ManyToOne
                                                                             ? c.ParentMap.PrimaryKey.Name
                                                                             : (c.Relationship == RelationshipType.OneToOne
                                                                                    ? c.OppositeColumn.Map.PrimaryKey.Name : null)
                                         })));
                }

                Console.Write((string)JsonConvert.SerializeObject(results));
            }
        }
    }
}