using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Engine.DML.Elements {
    using Dashing.Engine.Dialects;

    internal sealed class ConstantElement : ISqlElement {
        public string ParamName { get; set; }

        public object Value { get; set; }

        public ConstantElement(string paramName, object value) {
            this.ParamName = paramName;
            this.Value = value;
        }

        public void Append(StringBuilder stringBuilder, ISqlDialect dialect) {
            stringBuilder.Append(this.ParamName);
        }
    }
}
