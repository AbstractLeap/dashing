namespace TopHat.CodeGeneration {
    using TopHat.Configuration;

    internal interface ICodeGenerator {
        void Generate(IConfiguration configuration, CodeGeneratorConfig generatorConfig);
    }
}