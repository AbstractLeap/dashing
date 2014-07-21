namespace Dashing.CodeGeneration {
    using Dashing.Configuration;

    public interface ICodeGenerator {
        CodeGeneratorConfig Configuration { get; }

        IGeneratedCodeManager Generate(IConfiguration configuration);
    }
}