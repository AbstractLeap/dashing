namespace Dashing.Engine.DML {
    using System.Text;

    using Dashing.Engine.Dialects;

    internal interface ISqlElement {
        void Append(StringBuilder stringBuilder, ISqlDialect dialect);
    }
}