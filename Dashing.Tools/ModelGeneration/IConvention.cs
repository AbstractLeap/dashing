using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dashing.Tools.ModelGeneration
{
    
    public interface IConvention
    {
        string ClassNameForTable(string tableName);
    }
}
