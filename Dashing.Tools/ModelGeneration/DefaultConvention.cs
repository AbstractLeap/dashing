namespace Dashing.Tools.ModelGeneration {
    using Dashing.Extensions;

    public class DefaultConvention : IConvention {
        public string ClassNameForTable(string tableName) {
            var className = tableName.Singularize();

            // capitalise - helps with case-insensitivity of MySql on Windows
            return className[0].ToString().ToUpper() + className.Substring(1);
        }
    }
}