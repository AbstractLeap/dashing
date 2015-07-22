namespace Dashing.Dev.Weaver {
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration.Weaving;

    using Microsoft.Build.Framework;

    using Moq;

    internal class Program {
        private static int Main(string[] args) {
            var projectName = args[0];
            var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var config = currentDir.Split(Path.DirectorySeparatorChar).Last();
            var binPath = Path.Combine(currentDir, @"..\..\..\" + projectName + @"\bin\" + config);
            AssemblyLocation.Directory = binPath;
            var task = new ExtendDomain();
            var buildEngine = new Mock<IBuildEngine>();
            buildEngine.SetupGet(b => b.ProjectFileOfTaskNode)
                       .Returns(Path.Combine(currentDir, @"..\..\..\" + projectName + "\\" + projectName + ".csproj"));
            task.BuildEngine = buildEngine.Object;
            if (!task.Execute()) {
                return -1;
            }

            return 0;
        }
    }
}