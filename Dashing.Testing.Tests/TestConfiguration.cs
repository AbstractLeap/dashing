namespace Dashing.Testing.Tests {
    using Dashing.Configuration;
    using Dashing.Testing.Tests.TestDomain;

    public class TestConfiguration : BaseConfiguration {
        public TestConfiguration() {
            this.AddNamespaceOf<Post>();
            this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
        }
    }
}