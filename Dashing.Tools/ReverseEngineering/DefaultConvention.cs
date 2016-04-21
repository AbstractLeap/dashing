namespace Dashing.Tools.ReverseEngineering {
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Design.PluralizationServices;
    using System.Globalization;

    public class DefaultConvention : IConvention {
        private readonly PluralizationService pluralizer;

        /// <summary>
        /// </summary>
        /// <param name="extraPluralizationWords">
        ///     A string containing extra pluralization words
        ///     in the form Singular1,Plural1|Singular2,Plural2|Singular3,Plural3 ...
        /// </param>
        public DefaultConvention(IEnumerable<KeyValuePair<string, string>> extraPluralizationWords) {
            this.pluralizer = PluralizationService.CreateService(new CultureInfo("en-GB"));

            // ok, damned EnglishPluralizationService is an internal class so bit of reflection...
            var addWordMethod =
                typeof(PluralizationService).Assembly.GetType("System.Data.Entity.Design.PluralizationServices.EnglishPluralizationService")
                                            .GetMethod("AddWord");

            if (extraPluralizationWords != null) {
                try {
                    foreach (var pair in extraPluralizationWords) {
                        addWordMethod.Invoke(this.pluralizer, new object[] { pair.Key, pair.Value});
                    }
                }
                catch (Exception e) {
                    throw new Exception("At the moment only English pluralization is supported", e);
                }
            }
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