namespace Dashing.Dev.Weaver {
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration.Weaving;

    using Microsoft.Build.Framework;

    using Moq;

    internal class Program {
        private static int Main(string[] args) {
            var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var config = currentDir.Split(Path.DirectorySeparatorChar).Last();
            var binPath = Path.Combine(currentDir, @"..\..\..\" + args[0] + @"\bin\" + config);
            AssemblyLocation.Directory = binPath;
            var task = new ExtendDomain();
            task.BuildEngine = new Mock<IBuildEngine>().Object;
            if (!task.Execute()) {
                return -1;
            }

            return 0;
        }
    }
}