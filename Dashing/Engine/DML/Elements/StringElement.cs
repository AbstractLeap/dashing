namespace Dashing.Engine.DML.Elements {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal sealed class StringElement : ISqlElement {
        private readonly string sqlElement;

        public StringElement(string sqlElement) {
            this.sqlElement = sqlElement;
        }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect) {
            stringBuilder.Append(this.sqlElement);
        }
    }
}