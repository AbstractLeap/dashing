using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashing.Configuration;

namespace Dashing.Tools.Migration
{
    public interface IMigrator
    {
        string GenerateSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors);

        string GenerateNaiveSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors);
    }
}
