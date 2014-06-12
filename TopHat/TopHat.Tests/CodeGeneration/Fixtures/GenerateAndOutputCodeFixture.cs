namespace TopHat.Tests.CodeGeneration.Fixtures {
    using TopHat.CodeGeneration;

    public class GenerateAndOutputCodeFixture : GenerateCodeFixture {
        public GenerateAndOutputCodeFixture()
            : base(new CodeGeneratorConfig { CompileInDebug = true, OutputAssembly = true, OutputSourceCode = true }) { }
    }
}