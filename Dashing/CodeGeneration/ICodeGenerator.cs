namespace Dashing.CodeGeneration {
    using Dashing.Configuration;

    public interface ICodeGenerator {
        IGeneratedCodeManager Generate(IConfiguration configuration);
    }
}