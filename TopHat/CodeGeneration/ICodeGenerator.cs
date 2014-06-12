namespace TopHat.CodeGeneration {
    using TopHat.Configuration;

    public interface ICodeGenerator {
        IGeneratedCodeManager Generate(IConfiguration configuration);
    }
}