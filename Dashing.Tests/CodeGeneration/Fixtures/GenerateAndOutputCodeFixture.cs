namespace Dashing.Tests.CodeGeneration.Fixtures {
    using Dashing.CodeGeneration;

    public class GenerateAndOutputCodeFixture : GenerateCodeFixture {
        public GenerateAndOutputCodeFixture()
            : base(new CodeGeneratorConfig { CompileInDebug = true, OutputAssembly = true, OutputSourceCode = true }) {
        }
    }
}