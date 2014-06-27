using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TopHat.Configuration;

namespace TopHat.Tools.Migration
{
    public interface IMigrator
    {
        string GenerateSqlDiff(IConfiguration from, IConfiguration to, out IEnumerable<string> warnings, out IEnumerable<string> errors);
    }
}
