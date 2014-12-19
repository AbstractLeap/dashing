namespace Dashing.Tests.CodeGeneration.Fixtures {
    using Dashing.CodeGeneration;
    using Dashing.Tests.TestDomain;

    public class GenerateCodeFixture {
        public IGeneratedCodeManager CodeManager { get; private set; }

        public ICodeGenerator CodeGenerator { get; set; }

        public GenerateCodeFixture()
            : this(null) {}

        /// <remarks>We can't output the assembly or the source as the tests run in parallel, and IO doesn't!</remarks>
        protected GenerateCodeFixture(CodeGeneratorConfig generatorConfig) {
            this.CodeGenerator =
                new CodeGenerator(
                    generatorConfig ?? new CodeGeneratorConfig { CompileInDebug = true, OutputAssembly = false, OutputSourceCode = false },
                    new ProxyGenerator(),
                    new DapperWrapperGenerator());
            this.CodeManager = this.CodeGenerator.Generate(new CustomConfig());
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}