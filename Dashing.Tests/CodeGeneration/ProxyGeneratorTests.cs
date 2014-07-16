namespace Dashing.Tests.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Tests.Annotations;

    using Microsoft.AspNet.Identity;

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

        [Fact]
        public void IgnoreGetOnlyPropertiesInChangeTrackingProxy() {
            // assemble
            var target = MakeTarget();
            var maps = MakeMaps(typeof(ClassWithGetOnlyProperty));
            var config = new CodeGeneratorConfig();

            // act
            var results = target.GenerateProxies(config, maps);

            // assert
            Assert.NotNull(results);
            var changeTrackingProxy = results.ProxyTypes.Single(ctd => ctd.Name == typeof(ClassWithGetOnlyProperty).Name + config.TrackedClassSuffix);
            var members = new CodeTypeMember[changeTrackingProxy.Members.Count];
            changeTrackingProxy.Members.CopyTo(members, 0);
            Assert.False(members.Any(m => m.Name == "Id"));
        }

        [Fact]
        public void IgnoreGetOnlyPropertiesInUpdateProxy() {
            // assemble
            var target = MakeTarget();
            var maps = MakeMaps(typeof(ClassWithGetOnlyProperty));
            var config = new CodeGeneratorConfig();

            // act
            var results = target.GenerateProxies(config, maps);

            // assert
            Assert.NotNull(results);
            var updateProxy = results.ProxyTypes.Single(ctd => ctd.Name == typeof(ClassWithGetOnlyProperty).Name + config.UpdateClassSuffix);
            var members = new CodeTypeMember[updateProxy.Members.Count];
            updateProxy.Members.CopyTo(members, 0);
            Assert.False(members.Any(m => m.Name == "Id"));
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

        // make R# relax!
        // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
        // ReSharper disable once MemberCanBeProtected.Global
        private class ClassWithGetOnlyProperty {
            public virtual int UserId { get; [UsedImplicitly] set; }

            [UsedImplicitly]
            public virtual int Id {
                get {
                    return this.UserId;
                }
            }
        }
    }
}