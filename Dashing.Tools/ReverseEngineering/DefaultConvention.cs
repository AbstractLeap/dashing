namespace Dashing.Tools.ReverseEngineering {
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;

    public class DefaultConvention : IConvention {
        private readonly PluralizationService pluralizer;

        public DefaultConvention() {
            this.pluralizer = PluralizationService.CreateService(new CultureInfo("en-GB"));
        }

        public string PropertyNameForManyToOneColumnName(string columnName) {
            if (columnName.EndsWith("Id")) {
                return columnName.Substring(0, columnName.Length - 2);
            }

            return columnName;
        }

        public string ClassNameFor(string tableName) {
            return this.pluralizer.Singularize(tableName);
        }
    }
}