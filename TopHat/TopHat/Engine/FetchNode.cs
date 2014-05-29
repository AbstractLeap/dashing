namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;

    using TopHat.Configuration;

    public class FetchNode {
        public IColumn Column { get; set; }

        public string Alias { get; set; }

        public IDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; set; }

        public FetchNode() {
            this.Children = new Dictionary<string, FetchNode>();
        }
    }
}