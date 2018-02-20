namespace Dashing.Weaver.ConfigurationMetadataGeneration {
    using System.Reflection;

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
    }
}