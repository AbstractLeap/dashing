namespace Dashing.Engine.DML {
    using Dashing.Configuration;
    using Dashing.Extensions;

    public class FetchNode {
        public IColumn Column { get; set; }

        public string Alias { get; set; }

        public OrderedDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; set; }

        /// <summary>
        ///     Indicates whether the property here is being fetch or simply used in a where clause
        /// </summary>
        public bool IsFetched { get; set; }

        /// <summary>
        ///     This signature is used to select the correct mapper for dapper
        /// </summary>
        public string FetchSignature { get; set; }

        /// <summary>
        ///     This specifies the string to pass to dapper in order to split on
        /// </summary>
        public string SplitOn { get; set; }

        public int ContainedCollectionfetchesCount { get; set; }

        public FetchNode() {
            this.Children = new OrderedDictionary<string, FetchNode>();
        }
    }
}