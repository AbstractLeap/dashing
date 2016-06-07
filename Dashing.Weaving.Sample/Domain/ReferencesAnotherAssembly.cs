namespace Dashing.Weaving.Sample.Domain {
    using Dashing.Weaving.Sample2;

    public class ReferencesAnotherAssembly {
        public int Id { get; set; }

        public string Name { get; set; }

        public AnotherAssembliesClass TotherClass { get; set; }
    }
}