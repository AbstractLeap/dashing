namespace Dashing.Migration {
    using System.Text;

    public static class StringBuilderExtensions {
        public static void AppendSql(this StringBuilder builder, string sql) {
            builder.Append(sql);
            if (builder[builder.Length - 1] != ';') {
                builder.Append(";");
            }

            builder.AppendLine();
        }
    }
}