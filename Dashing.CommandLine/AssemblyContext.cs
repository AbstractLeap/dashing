namespace Dashing.CommandLine {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Extensions;
    
    public static class AssemblyContext {
        public static Assembly LoadFile(string assemblyPath) {
            return Assembly.LoadFile(assemblyPath);
        }

        public static TInterfaceType LoadType<TInterfaceType>(string assemblyPath, string configurationFullName)
            where TInterfaceType : class {
            if (!File.Exists(assemblyPath)) {
                throw new Exception($"Unable to find assembly at {assemblyPath}");
            }

            if (!Path.IsPathRooted(assemblyPath)) {
                assemblyPath = Path.GetFullPath(assemblyPath);
            }

            var assembly = LoadFile(assemblyPath);
            var type = assembly.GetLoadableTypes()
                               .SingleOrDefault(t => t.FullName == configurationFullName);
            if (type == null) {
                throw new Exception($"Unable to find configuration of type {configurationFullName} in {assemblyPath}");
            }

            if (!(Activator.CreateInstance(type) is TInterfaceType instance)) {
                throw new Exception($"The type {configurationFullName} does not implement IConfiguration");
            }

            return instance;
        }
    }
}