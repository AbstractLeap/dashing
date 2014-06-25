using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader;

namespace TopHat.Configuration.ReverseEngineer
{
    public interface IEngineer
    {
        IEnumerable<IMap> ReverseEngineer(IDatabaseReader databaseReader);
    }
}
