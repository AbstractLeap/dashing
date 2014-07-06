namespace Dashing.Tools.ReverseEngineering {
    public interface IConvention {
        string PropertyNameForManyToOneColumnName(string columnName);
    }
}