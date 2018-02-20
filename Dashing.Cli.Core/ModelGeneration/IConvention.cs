namespace Dashing.ModelGeneration {
    public interface IConvention {
        string ClassNameForTable(string tableName);
    }
}