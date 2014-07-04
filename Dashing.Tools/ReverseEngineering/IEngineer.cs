using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader;
using Dashing.Configuration;

namespace Dashing.Tools.ReverseEngineering
{
    public interface IEngineer
    {
        IEnumerable<IMap> ReverseEngineer(DatabaseSchemaReader.DataSchema.DatabaseSchema schema);
    }
}
