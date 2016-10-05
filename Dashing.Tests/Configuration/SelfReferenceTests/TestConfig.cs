namespace Dashing.Tests.Configuration.SelfReferenceTests {
    using Dashing.Configuration;
    using Dashing.Tests.Configuration.SelfReferenceTests.Domain;

    public class TestConfig : BaseConfiguration {
        public TestConfig() {
            this.AddNamespaceOf<Post>();
        }
    }
}