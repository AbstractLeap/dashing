namespace Dashing.Console.Settings {
    using System.Collections.Generic;
    using System.Linq;

    internal class ReverseEngineerSettings {
        public string GeneratedNamespace { get; set; }

        public string ExtraPluralizationWords { get; set; }

        public string TablesToIgnore { private get; set; }

        public string IndexesToIgnore { get; set; }

        public IEnumerable<string> GetIndexesToIgnore() {
            return this.IndexesToIgnore.Split(',', '|').Select(s => s.Trim());
        }

        public IEnumerable<string> GetTablesToIgnore() {
            IEnumerable<string> tablesToIgnore = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.TablesToIgnore)) {
                tablesToIgnore = this.TablesToIgnore.Split(',').Select(s => s.Trim());
            }

            return tablesToIgnore.Union(new[] { "sysdiagrams" });
        }

        public IEnumerable<KeyValuePair<string, string>> GetExtraPluralizationWords() {
            var result = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(this.ExtraPluralizationWords)) {
                return result;
            }

            var pairs = this.ExtraPluralizationWords.Split('|');
            foreach (var pair in pairs) {
                var words = pair.Split(',');
                if (words.Length == 2) {
                    result.Add(new KeyValuePair<string, string>(words[0], words[1]));
                }
            }

            return result;
        }
    }
}