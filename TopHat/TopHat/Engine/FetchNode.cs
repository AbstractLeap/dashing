namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;

    public class FetchNode {
        public string PropertyName { get; set; }

        public Type PropertyType { get; set; }

        public string Alias { get; set; }

        public IDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; set; }

        public FetchNode() {
            this.Children = new Dictionary<string, FetchNode>();
        }
    }
}