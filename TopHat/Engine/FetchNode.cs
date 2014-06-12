namespace TopHat.Engine {
    using System.Collections.Generic;

    using TopHat.Configuration;

    public class FetchNode {
        public IColumn Column { get; set; }

        public string Alias { get; set; }

        public IDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; set; }

        /// <summary>
        ///     This signature is used to select the correct mapper for dapper
        /// </summary>
        public string FetchSignature { get; set; }

        /// <summary>
        ///     This specifies the string to pass to dapper in order to split on
        /// </summary>
        public string SplitOn { get; set; }

        public FetchNode() {
            this.Children = new Dictionary<string, FetchNode>();
        }
    }
}