namespace Dashing.Engine.DML.Elements
{
    using System.Text;

    using Dashing.Engine.Dialects;
    internal sealed class SpecificParameterColumnElement : ISqlElement
    {
        private readonly string alias;
        private readonly string columnName;

        public SpecificParameterColumnElement(string alias, string columnName)
        {
            this.alias = alias;
            this.columnName = columnName;
        }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect)
        {
            if (!string.IsNullOrWhiteSpace(this.alias))
            {
                stringBuilder.Append(this.alias).Append(".");
            }

            dialect.AppendQuotedName(stringBuilder, this.columnName);
        }
    }
}