namespace Dashing.Engine.DML.Elements {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal interface ISqlElement {
        void Append(StringBuilder stringBuilder, ISqlDialect dialect, IAliasProvider aliasProvider);
    }
}