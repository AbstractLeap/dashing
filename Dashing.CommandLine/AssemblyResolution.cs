namespace Dashing.CommandLine {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    
    public static class AssemblyResolution {
        public static void Configure(IList<string> assemblyPaths) {
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
        }
    }
}