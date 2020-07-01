namespace Dashing.Engine.DML.Elements {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal sealed class ColumnElement : ISqlElement {
        private readonly string columnName;

        public ColumnElement(BaseQueryNode queryNode, string columnName, bool isRoot) {
            this.QueryNode = queryNode;
            this.columnName = columnName;
            this.IsRoot = isRoot;
        }

        public bool IsRoot { get; set; }

        public BaseQueryNode QueryNode { get; set; }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect, IAliasProvider aliasProvider) {
            if (this.QueryNode != null) {
                stringBuilder.Append(aliasProvider.GetAlias(this.QueryNode)).Append(".");
            }

            dialect.AppendQuotedName(stringBuilder, this.columnName);
        }
    }
}