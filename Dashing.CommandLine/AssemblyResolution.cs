namespace Dashing.CommandLine {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

#if COREFX
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyModel;
#endif

    public static class AssemblyResolution {
        public static void Configure(IList<string> assemblyPaths) {
#if COREFX
            AssemblyLoadContext.Default.Resolving += (context, name) =>
                {
                    var dependencies = DependencyContext.Default.RuntimeLibraries;
                    foreach(var library in dependencies) {
                        if (library.Name == name.Name) {
                            return context.LoadFromAssemblyName(new AssemblyName(library.Name));
                        }
                    }

                    // look on disk
                    foreach (var path in assemblyPaths) {
                        var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{name.Name}.{ext}");
                        foreach (var attempt in attempts) {
                            if (File.Exists(attempt)) {
                                return AssemblyContext.LoadFile(attempt);
                            }
                        }
                    }
            
                    return null;
                };
#else
            AppDomain.CurrentDomain.AssemblyResolve += (sender, iargs) => {
                var assemblyName = new AssemblyName(iargs.Name);

                // look in app domain
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                                      .SingleOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    return loaded;
                }

                // we couldn't find it, look on disk
                foreach (var path in assemblyPaths) {
                    var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{assemblyName.Name}.{ext}");
                    foreach (var attempt in attempts) {
                        if (File.Exists(attempt)) {
                            return Assembly.LoadFile(Path.GetFullPath(attempt));
                        }
                    }
                }
                
                return null;
            };
#endif
        }
    }
}