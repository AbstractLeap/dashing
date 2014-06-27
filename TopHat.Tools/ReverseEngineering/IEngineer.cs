using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader;
using TopHat.Configuration;

namespace TopHat.Tools.ReverseEngineering
{
    public interface IEngineer
    {
        IEnumerable<IMap> ReverseEngineer(IDatabaseReader databaseReader);
    }
}
