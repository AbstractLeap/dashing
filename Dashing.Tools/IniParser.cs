namespace Dashing.Tools {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class IniParser {
        public static IDictionary<string, IDictionary<string, object>> Parse(string filePath) {
            var result = new Dictionary<string, IDictionary<string, object>>();
            var lastDic = new Dictionary<string, object>();
            using (var streamReader = new StreamReader(filePath)) {
                string line = null;
                while ((line = streamReader.ReadLine()) != null) {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) {
                        continue;
                    }

                    if (line[0] == ';' || line[0] == '#') {
                        // comment
                        continue;
                    }

                    if (line[0] == '[') {
                        // new category
                        lastDic = new Dictionary<string, object>();
                        result.Add(line.Substring(1, line.Length - 2), lastDic);
                        continue;
                    }

                    if (result.Count == 0) {
                        throw new InvalidDataException(
                            "The ini file must enclose all values in categories i.e. put a [Category] above");
                    }

                    var splitPos = line.IndexOf('=');
                    if (splitPos == -1) {
                        throw new InvalidDataException(
                            "All settings must be of the form \"key = value\"");
                    }

                    lastDic.Add(
                        line.Substring(0, splitPos).Trim(),
                        GetValue(line.Substring(splitPos + 1).Trim()));
                }
            }

            return result;
        }

        public static T AssignTo<T>(IEnumerable<KeyValuePair<string, object>> configSection, T entity) {
            foreach (var kvp in configSection) {
                var prop = typeof(T).GetProperty(kvp.Key);
                if (prop == null) {
                    throw new InvalidDataException(
                        "For using AssignTo the properties in the ini file must match the properties in the class");
                }

                prop.SetValue(entity, kvp.Value);
            }

            return entity;
        }

        private static object GetValue(string value) {
            if (new[] { "yes", "true" }.Contains(value.ToLower())) {
                return true;
            }

            if (new[] { "no", "false" }.Contains(value.ToLower())) {
                return false;
            }

            if (value[0] == '"') {
                // remove enclosing quotes
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}