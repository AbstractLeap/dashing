namespace Dashing.Tools.ModelGeneration {
    public interface IConvention {
        string ClassNameForTable(string tableName);
    }
}