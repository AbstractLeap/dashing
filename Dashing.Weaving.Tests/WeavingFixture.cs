namespace Dashing.Weaving.Tests {
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Policy;

    using Dashing.CodeGeneration.Weaving;

    using Microsoft.Build.Framework;

    using Moq;

    using Xunit;

    [Collection("Weaving Tests")]
    public class WeavingFixture {
        public WeavingFixture() {
            // do the re-writing
#if DEBUG
            AssemblyLocation.Directory = @"D:\Projects\Dashing\Dashing.Weaving.Tests\bin\Debug\";
#else
            AssemblyLocation.Directory = @"D:\Projects\Dashing\Dashing.Weaving.Tests\bin\Release\";
#endif
            var task = new ExtendDomain();
            task.BuildEngine = new Mock<IBuildEngine>().Object;
            Assert.True(task.Execute());
        }
    }
}