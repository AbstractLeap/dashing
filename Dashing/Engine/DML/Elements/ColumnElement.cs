namespace Dashing.Engine.DML.Elements
{
    using System.Text;

    using Dashing.Engine.Dialects;

    internal sealed class ColumnElement : ISqlElement {
        private readonly string columnName;

        public ColumnElement(FetchNode node, string columnName, bool isRoot) {
            this.Node = node;
            this.columnName = columnName;
            this.IsRoot = isRoot;
        }

        public bool IsRoot { get; set; }

        public FetchNode Node { get; set; }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect) {
            if (this.Node != null) {
                stringBuilder.Append(this.Node.Alias).Append(".");
            }

            dialect.AppendQuotedName(stringBuilder, this.columnName);
        }
    }
}