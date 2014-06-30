using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Tools.ReverseEngineering
{
    public interface IConvention
    {
        string PropertyNameForManyToOneColumnName(string columnName);
    }
}
