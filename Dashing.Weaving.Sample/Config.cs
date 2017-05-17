namespace Dashing.Weaving.Sample {
    using Dashing.Configuration;
    using Dashing.Weaving.Sample.Domain;
    using Dashing.Weaving.Sample2;

    public class Config : BaseConfiguration {
        public Config() {
            this.AddNamespaceOf<Foo>();
            this.Add<AnotherAssembliesClass>();
        }
    }
}