namespace Dashing.Weaving.Sample {
    using Dashing.Configuration;
    using Dashing.Weaving.Sample.Domain;
    using Dashing.Weaving.Sample.Domain.Tracking;
    using Dashing.Weaving.Sample2;

    public class Config : BaseConfiguration {
        public Config() {
            this.AddNamespaceOf<Foo>();
            this.AddNamespaceOf<GuidPk>();
            this.Add<AnotherAssembliesClass>();
            this.Setup<Foo>()
                .Property(f => f.Ducks)
                .ShouldWeavingInitialiseListInConstructor = true;
            this.Setup<Whopper>()
                .Property(w => w.Ducks)
                .ShouldWeavingInitialiseListInConstructor = true;
        }
    }
}