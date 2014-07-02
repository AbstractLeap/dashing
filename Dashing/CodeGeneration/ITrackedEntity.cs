namespace Dashing.CodeGeneration {
    using System.Collections.Generic;

    public interface ITrackedEntity {
        bool IsTracking { get; set; }

        ISet<string> DirtyProperties { get; set; }

        IDictionary<string, object> OldValues { get; set; }

        IDictionary<string, object> NewValues { get; set; }

        IDictionary<string, IList<object>> AddedEntities { get; set; }

        IDictionary<string, IList<object>> DeletedEntities { get; set; }
    }
}