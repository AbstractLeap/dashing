namespace Dashing.Tools.ReverseEngineering {
    public interface IConvention {
        string PropertyNameForManyToOneColumnName(string columnName);

        string ClassNameFor(string tableName);
    }
}