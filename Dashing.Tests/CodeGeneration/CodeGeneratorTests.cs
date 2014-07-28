namespace Dashing.Tests.CodeGeneration {
    using Dashing.CodeGeneration;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class CodeGeneratorTests {
        [Fact]
        public void GeneratesTehCodez() {
            var config = new CodeGeneratorConfig {
                CompileInDebug = true, 
                OutputAssembly = true, 
                OutputSourceCode = true
            };

            var codeGenerator = new CodeGenerator(config, new ProxyGenerator(), new DapperWrapperGenerator());
            codeGenerator.Generate(new CustomConfig());
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}