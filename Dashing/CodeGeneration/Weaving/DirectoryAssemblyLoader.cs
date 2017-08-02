namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.IO;
    using System.Reflection;

    //public class DirectoryAssemblyLoader : IAssemblyLoader
    //{
    //    private readonly string directory;

    //    public DirectoryAssemblyLoader(string assemblyFilePath)
    //    {
    //        this.directory = Path.GetDirectoryName(assemblyFilePath);
    //    }

    //    public Assembly Load(AssemblyName assemblyName)
    //    {
    //        var possibleFilePaths = new[] {
    //                                          this.directory + "\\" + assemblyName.Name, this.directory + "\\" + assemblyName.Name + ".dll",
    //                                          this.directory + "\\" + assemblyName.Name + ".exe"
    //                                      };
    //        foreach (var possibleFilePath in possibleFilePaths)
    //        {
    //            if (File.Exists(possibleFilePath))
    //            {
    //                return PlatformServices.Default.AssemblyLoadContextAccessor.Default.LoadFile(possibleFilePath);
    //            }
    //        }

    //        throw new TypeLoadException("Unable to find an assembly for " + assemblyName.FullName);
    //    }

    //    public IntPtr LoadUnmanagedLibrary(string name)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}