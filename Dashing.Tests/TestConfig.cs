namespace Dashing.Tests {
    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;

    public class TestConfig : BaseConfiguration {
        public TestConfig() {
            this.AddNamespaceOf<Post>();
        }
    }
}