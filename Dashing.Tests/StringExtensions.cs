namespace Dashing.Tools.Tests {
    public static class StringExtensions {
        public static string NormalizeNewlines(this string value) {
            return value.Replace(@"\r\n", @"\n");
        }
    }
}