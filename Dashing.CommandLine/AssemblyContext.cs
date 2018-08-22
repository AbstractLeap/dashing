namespace Dashing.CommandLine {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Extensions;

#if COREFX
    using System.Runtime.Loader;
#endif

    public static class AssemblyContext {
        public static Assembly LoadFile(string assemblyPath) {
#if COREFX
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#else
            return Assembly.LoadFile(assemblyPath);
#endif
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