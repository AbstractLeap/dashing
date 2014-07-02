using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.CodeGeneration {
    public interface IUpdateClass {
        IList<string> UpdatedProperties { get; set; }
    }
}
