namespace Dashing.Extensions {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class TopologicalOrderResult {
        public IEnumerable<IMap> OrderedMaps { get; set; }

        public Tuple<IMap, IMap> OneToOneMapPairs { get; set; }

        public IEnumerable<IMap> SelfReferencingMaps { get; set; }
    }
}