using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Tools.ReverseEngineering {
    interface IReverseEngineeringColumn {
        string ForeignKeyTableName { get; set; }

        IDictionary<string, Type> TypeMap { get; set; }
    }
}
