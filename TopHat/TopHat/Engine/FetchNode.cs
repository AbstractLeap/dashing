namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;

    using TopHat.Configuration;

    public class FetchNode {
        public IColumn Column { get; set; }

        public string Alias { get; set; }

        public IDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; set; }

        /// <summary>
        /// This signature is used to select the correct mapper for dapper
        /// </summary>
        public string FetchSignature { get; set; }

        public FetchNode() {
            this.Children = new Dictionary<string, FetchNode>();
        }
    }
}