using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGeneration
{
    public interface ITrackedEntity
    {
        bool IsTracking { get; set; }

        ISet<string> DirtyProperties { get; set; }

        IDictionary<string, object> OldValues { get; set; }

        IDictionary<string, IList<object>> AddedEntities { get; set; }

        IDictionary<string, IList<object>> DeletedEntities { get; set; }
    }
}