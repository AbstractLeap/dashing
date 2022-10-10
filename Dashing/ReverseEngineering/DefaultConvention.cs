namespace Dashing.ReverseEngineering {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Extensions;

    public class DefaultConvention : IConvention {
        private readonly IDictionary<string, string> extraWords;

        /// <summary>
        /// </summary>
        /// <param name="extraPluralizationWords">
        ///     A string containing extra pluralization words
        ///     in the form Singular1,Plural1|Singular2,Plural2|Singular3,Plural3 ...
        /// </param>
        public DefaultConvention(IEnumerable<KeyValuePair<string, string>> extraPluralizationWords) {
            this.extraWords = (extraPluralizationWords ?? new Dictionary<string, string>()).ToDictionary(k => k.Value.ToLowerInvariant(), k => k.Key);
        }

        public string PropertyNameForManyToOneColumnName(string columnName) {
            if (columnName.EndsWith("Id")) {
                return columnName.Substring(0, columnName.Length - 2);
            }

            return columnName;
        }

        public string ClassNameFor(string tableName) {
            if (this.extraWords.ContainsKey(tableName.ToLowerInvariant())) {
                return this.extraWords[tableName.ToLowerInvariant()];
            }

            return tableName.Singularize();
        }
    }
}