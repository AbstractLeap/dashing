namespace Dashing.Weaving.Tests {
    using System;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.CodeGeneration.Weaving;

    using Microsoft.Build.Framework;

    using Moq;

    using Xunit;

    public class Tests {
        [Fact]
        public void ItWorks() {
            // do the re-writing
            AssemblyLocation.Directory = @"D:\Projects\Dashing\Dashing.Weaving.Sample\bin\Debug\";
            var task = new ExtendDomain();
            task.BuildEngine = new Mock<IBuildEngine>().Object;
                task.Execute();

            // load the program and test
            var assembly = Assembly.LoadFile(AssemblyLocation.Directory + "Dashing.Weaving.Sample.exe");
            var fooType = assembly.GetTypes().Single(t => t.Name == "Foo" && t.Namespace.Contains("Domain"));
            var foo = Activator.CreateInstance(fooType);
            Assert.True(foo is ITrackedEntity);
        } 
    }
}