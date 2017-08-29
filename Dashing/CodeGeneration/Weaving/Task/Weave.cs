namespace Dashing.CodeGeneration.Weaving.Task {
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
#if COREFX
    using System.Runtime.Loader;    
#endif

    using Dashing.Configuration;

    using Microsoft.Build.Framework;

    using Newtonsoft.Json;

    public class Weave : ConfigBasedTask {
        public string Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// see https://github.com/dotnet/MVPSummitHackathon2016/blob/master/SampleTargets.PackerTarget/Packer.cs
        ///     https://github.com/bling/dependencypropertyweaver/blob/master/src/DependencyPropertyWeaver/DependencyPropertyWeaverTask.cs
        ///     https://github.com/dotnet/MVPSummitHackathon2016/blob/master/dotnet-packer/Program.cs
		///		http://thomasardal.com/msbuild-tutorial/#Custom_MSBuild_Tasks
        /// </remarks>
        /// <returns></returns>
        public override bool Execute() {
            //// load the assembly
            //var assembly = AssemblyDefinition.ReadAssembly(
            //    this.ConfigurationAssemblyPath,
            //    new ReaderParameters { });

            //var typesToProcess = JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<ColumnDefinition>>>(this.Configuration);

            //// do some weaving
            //foreach (var typeFullName in typesToProcess)
            //{
            //    var typeDefinition = assembly.MainModule.GetType(typeFullName.Key);
            //    foreach (var weaver in weavers)
            //    {
            //        weaver.Weave(assembly, typeDefinition, typeFullName.Value);
            //    }
            //}

            //assembly.Write(peFilePath);
            return true;
        }
    }

    public class GetConfig : ConfigBasedTask {
        [Output]
        public string Configuration { get; set; }

        public override bool Execute() {
            // load up the file with the config, find the config, instantiate and return as json
            var assembly = Context.LoadFile(this.ConfigurationAssemblyPath);
            if (assembly == null) {
                this.Error($"Unable to load assembly at {this.ConfigurationAssemblyPath}");
                return false;
            }

            var configurationType = assembly.GetLoadableTypes().SingleOrDefault(t => t.FullName == this.ConfigurationFullName);
            if (configurationType == null) {
                this.Error($"Unable to find type {this.ConfigurationFullName} in {this.ConfigurationAssemblyPath}");
                return false;
            }

            var configuration = (IConfiguration)Activator.CreateInstance(configurationType);
            this.Configuration = JsonConvert.SerializeObject(configuration.Maps.ToDictionary(
                map => map.Type.FullName,
                map => map.Columns.Where(c => !c.Value.IsIgnored)
                        .Select(c => c.Value)
                        .Select(c => new ColumnDefinition
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
                        })
                ));
            return true;
        }
    }

#if COREFX
    public abstract class ConfigBasedTask : Microsoft.Build.Utilities.Task
#else
    [LoadInSeparateAppDomain]
    [Serializable]
    public abstract class ConfigBasedTask : Microsoft.Build.Utilities.AppDomainIsolatedTask
#endif
    { 
        /// <summary>
        /// The fullname of the type that contains the configuration
        /// </summary>
        [Required]
        public string ConfigurationFullName { get; set; }
        
        /// <summary>
        /// The path to the assembly that contains the configuration
        /// </summary>
        [Required]
        public string ConfigurationAssemblyPath { get; set; }

        protected void Log(string message) {
            if (this.BuildEngine != null) {
                base.Log.LogMessage(message);
            } else {
                Trace.WriteLine(message);
            }
        }

        protected void Error(string message) {
            if (this.BuildEngine != null) {
                base.Log.LogError(message);
            } else {
                Trace.WriteLine(message);
            }
        }
    }

    public static class Context {
        public static Assembly LoadFile(string assemblyPath) {
#if COREFX
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#else
            return Assembly.LoadFile(assemblyPath);
#endif
        }
    }
}