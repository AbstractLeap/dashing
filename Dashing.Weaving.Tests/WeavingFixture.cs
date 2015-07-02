namespace Dashing.Weaving.Tests {
    using System.Security.Policy;

    using Dashing.CodeGeneration.Weaving;

    using Microsoft.Build.Framework;

    using Moq;

    using Xunit;

    [Collection("Weaving Tests")]
    public class WeavingFixture {
        public WeavingFixture() {
            // do the re-writing
            AssemblyLocation.Directory = @"D:\Projects\Dashing\Dashing.Weaving.Tests\bin\Debug\";
            var task = new ExtendDomain();
            task.BuildEngine = new Mock<IBuildEngine>().Object;
            Assert.True(task.Execute());
        }
    }
}