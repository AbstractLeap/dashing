namespace Dashing.Engine.DML {
    using System;

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
        ///     If true then this property can be inner joined as it, or one of it's children,
        ///     is used in a where clause
        /// </summary>
        public bool InferredInnerJoin { get; set; }

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

        /// <summary>
        ///     Clones a parent fetch node
        /// </summary>
        /// <returns></returns>
        public FetchNode Clone() {
            if (this.Parent != null) {
                throw new NotSupportedException();
            }

            if (this.Column != null) {
                throw new NotSupportedException();
            }

            var clone = new FetchNode {
                                          Alias = this.Alias,
                                          ContainedCollectionfetchesCount = this.ContainedCollectionfetchesCount,
                                          FetchSignature = this.FetchSignature,
                                          InferredInnerJoin = this.InferredInnerJoin,
                                          IsFetched = this.IsFetched,
                                          SplitOn = this.SplitOn
                                      };
            var childCopy = new OrderedDictionary<string, FetchNode>();
            foreach (var keyValue in this.Children) {
                childCopy.Add(keyValue.Key, keyValue.Value);
            }

            clone.Children = childCopy;
            return clone;
        }
    }
}