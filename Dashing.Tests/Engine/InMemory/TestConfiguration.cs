namespace Dashing.Tests.Engine.InMemory {
    using Dashing.Configuration;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    public class TestConfiguration : BaseConfiguration {
        public TestConfiguration() {
            this.AddNamespaceOf<Post>();
            this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
        }
    }
}