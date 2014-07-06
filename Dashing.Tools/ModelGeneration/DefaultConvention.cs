namespace Dashing.Tools.ModelGeneration {
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;

    public class DefaultConvention : IConvention {
        private readonly PluralizationService pluralizationService;

        public DefaultConvention() {
            this.pluralizationService = PluralizationService.CreateService(new CultureInfo("en-gb"));
        }

        public string ClassNameForTable(string tableName) {
            var className = this.pluralizationService.Singularize(tableName);

            // capitalise - helps with case-insensitivity of MySql on Windows
            return className[0].ToString().ToUpper() + className.Substring(1);
        }
    }
}