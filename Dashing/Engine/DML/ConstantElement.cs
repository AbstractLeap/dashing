namespace Dashing.Engine.DML {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal class ConstantElement : ISqlElement {
        private readonly string sqlElement;

        public ConstantElement(string sqlElement) {
            this.sqlElement = sqlElement;
        }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect) {
            stringBuilder.Append(this.sqlElement);
        }
    }
}