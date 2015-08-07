namespace Dashing.CodeGeneration.Weaving {
    using System.IO;

    public class AssemblyLocation {
        static AssemblyLocation() {
            var assembly = typeof(AssemblyLocation).Assembly;

            var path = assembly.Location
                .Replace("file:///", "")
                .Replace("file://", "")
                .Replace(@"file:\\\", "")
                .Replace(@"file:\\", "");

            Directory = Path.GetDirectoryName(path);
        }

        public static string Directory;
    }
}