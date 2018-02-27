namespace Dashing.IntegrationTests.Setup {
    using Dashing.Configuration;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More;

    class Configuration : BaseConfiguration {
        public Configuration() {
            this.AddNamespaceOf<Post>();
            this.AddNamespaceOf<Questionnaire>();
        }
    }
}