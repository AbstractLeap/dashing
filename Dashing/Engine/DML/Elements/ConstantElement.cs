namespace Dashing.Engine.DML.Elements {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal sealed class ConstantElement : ISqlElement {
        public string ParamName { get; set; }

        public object Value { get; set; }

        public ConstantElement(string paramName, object value) {
            this.ParamName = paramName;
            this.Value = value;
        }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect, IAliasProvider aliasProvider) {
            stringBuilder.Append(this.ParamName);
        }
    }
}