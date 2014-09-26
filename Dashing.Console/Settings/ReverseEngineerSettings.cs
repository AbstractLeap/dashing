namespace Dashing.Console.Settings {
    using System.Collections.Generic;
    using System.Linq;

    internal class ReverseEngineerSettings {
        public string GeneratedNamespace { get; set; }

        public string ExtraPluralizationWords { get; set; }

        public string TablesToIgnore { private get; set; }

        public IEnumerable<string> GetTablesToIgnore() {
            IEnumerable<string> tablesToIgnore = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.TablesToIgnore)) {
                tablesToIgnore = this.TablesToIgnore.Split(',').Select(s => s.Trim());
            }

            return tablesToIgnore.Union(new[] { "sysdiagrams" });
        }
    }
}