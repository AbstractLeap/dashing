namespace Dashing.Extensions {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class TopologicalOrderResult {
        public IEnumerable<IMap> OrderedMaps { get; set; }

        public IEnumerable<IMap> OneToOneMaps { get; set; }

        public IEnumerable<IMap> SelfReferencingMaps { get; set; }
    }
}