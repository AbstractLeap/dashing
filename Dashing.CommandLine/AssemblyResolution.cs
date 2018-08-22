namespace Dashing.CommandLine {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Serilog;

#if COREFX
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyModel;
#endif

    public static class AssemblyResolution {
        public static void Configure(IList<string> assemblyPaths) {
#if COREFX
            AssemblyLoadContext.Default.Resolving += (context, name) =>
                {
                    Log.Logger.Debug("Resolution failed for Assembly: {Name}", name);
                    var dependencies = DependencyContext.Default.RuntimeLibraries;
                    foreach(var library in dependencies) {
                        Log.Logger.Debug("Looking for {Name} in dependency {Library}", name, library);
                        if (library.Name == name.Name) {
                            Log.Logger.Debug("Found {Name} in dependency {Library}", name, library);
                            return context.LoadFromAssemblyName(new AssemblyName(library.Name));
                        }
                    }

                    // look on disk
                    foreach (var path in assemblyPaths) {
                        var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{name.Name}.{ext}");
                        foreach (var attempt in attempts) {
                            Log.Logger.Debug("Looking for {Name} at {attempt}", name, attempt);
                            if (File.Exists(attempt)) {
                                Log.Logger.Debug("Found {Name} at {attempt}, loading now", name, attempt);
                                return AssemblyContext.LoadFile(attempt);
                            }
                        }
                    }
            
                    Log.Logger.Debug("Failed to find {Name}", name);
                    return null;
                };
#else
            AppDomain.CurrentDomain.AssemblyResolve += (sender, iargs) => {
                var assemblyName = new AssemblyName(iargs.Name);
                Log.Logger.Debug("Resolution failed for Assembly: {AssemblyName}", assemblyName);

                // look in app domain
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                                      .SingleOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    Log.Logger.Debug("Found assembly {AssemblyName} in the current app domain", assemblyName);
                    return loaded;
                }

                // we couldn't find it, look on disk
                foreach (var path in assemblyPaths) {
                    var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{assemblyName.Name}.{ext}");
                    foreach (var attempt in attempts) {
                        Log.Logger.Debug("Looking for {AssemblyName} at {attempt}", assemblyName, attempt);
                        if (File.Exists(attempt)) {
                            Log.Logger.Debug("Found {AssemblyName} at {attempt}, loading now", assemblyName, attempt);
                            return Assembly.LoadFile(attempt);
                        }
                    }
                }

                Log.Logger.Debug("Failed to find {AssemblyName}", assemblyName);
                return null;
            };
#endif
        }
    }
}