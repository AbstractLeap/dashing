namespace Dashing.Weaving.Sample.Target {
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    public class Foo : ITrackedEntity {
        public int FooId { get; set; }

        public bool IsTracking { get; set; }

        public ISet<string> DirtyProperties { get; set; }

        public IDictionary<string, object> OldValues { get; set; }

        public IDictionary<string, object> NewValues { get; set; }

        public IDictionary<string, IList<object>> AddedEntities { get; set; }

        public IDictionary<string, IList<object>> DeletedEntities { get; set; }
    }
}