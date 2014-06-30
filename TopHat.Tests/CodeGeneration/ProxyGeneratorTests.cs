namespace TopHat.Tests.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNet.Identity;

    using TopHat.CodeGeneration;
    using TopHat.Configuration;

    using Xunit;

    public class ProxyGeneratorTests {
        [Fact]
        public void IncludesNamespacesOfBaseTypes() {
            // assemble
            var target = MakeTarget();
            var maps = MakeMaps(typeof(ConcreteUserInt));
            var config = new CodeGeneratorConfig();

            // act
            var results = target.GenerateProxies(config, maps);

            // assert
            Assert.NotNull(results);
            Assert.Contains(typeof(IUser<int>).Namespace, results.NamespaceImports.Select(nsi => nsi.Namespace));
        }

        [Fact]
        public void IncludesReferencesToBaseTypeAssemblies() {
            // assemble
            var target = MakeTarget();
            var maps = MakeMaps(typeof(ConcreteUserInt));
            var config = new CodeGeneratorConfig();

            // act
            var results = target.GenerateProxies(config, maps);

            // assert
            Assert.NotNull(results);
            Assert.Contains(typeof(IUser<int>).Assembly.Location, results.ReferencedAssemblyLocations);
        }

        private static Dictionary<Type, IMap> MakeMaps(params Type[] types) {
            var mapper = MakeMapper();
            var maps = types.ToDictionary(t => t, mapper.MapFor);
            return maps;
        }

        private static DefaultMapper MakeMapper() {
            return new DefaultMapper(new DefaultConvention());
        }

        private static ProxyGenerator MakeTarget() {
            return new ProxyGenerator();
        }

        private class ConcreteUserInt : IUser<int> {
            public int Id { get; private set; }

            public string UserName { get; set; }
        }
    }
}
