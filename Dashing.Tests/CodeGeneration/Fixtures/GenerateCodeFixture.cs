namespace Dashing.Tests.CodeGeneration.Fixtures {
    using Moq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Tests.TestDomain;

    public class GenerateCodeFixture {
        private readonly Mock<IEngine> engine = new Mock<IEngine>();

        public IGeneratedCodeManager CodeManager { get; private set; }

        public GenerateCodeFixture() : this(null) { }

        // ReSharper disable once MemberCanBeProtected.Global (instantiated by xUnit)
        public GenerateCodeFixture(CodeGeneratorConfig generatorConfig) {
            var codeGenerator = new CodeGenerator(generatorConfig ?? new CodeGeneratorConfig(), new ProxyGenerator());
            this.CodeManager = codeGenerator.Generate(new CustomConfig(this.engine.Object));
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig(IEngine engine)
                : base(engine, string.Empty) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}