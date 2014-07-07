namespace Dashing.Tests.CodeGeneration.Fixtures {
    using Dashing.CodeGeneration;
    using Dashing.Tests.TestDomain;

    public class GenerateCodeFixture {
        public IGeneratedCodeManager CodeManager { get; private set; }

        public GenerateCodeFixture()
            : this(null) { }

        protected GenerateCodeFixture(CodeGeneratorConfig generatorConfig) {
            var codeGenerator = new CodeGenerator(generatorConfig ?? new CodeGeneratorConfig(), new ProxyGenerator());
            this.CodeManager = codeGenerator.Generate(new CustomConfig());
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}