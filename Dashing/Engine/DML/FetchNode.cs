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

            return InnerClone(this, null);
        }

        private static FetchNode InnerClone(FetchNode progenitor, FetchNode parent) {
            var clone = new FetchNode {
                                          Alias = progenitor.Alias,
                                          ContainedCollectionfetchesCount = progenitor.ContainedCollectionfetchesCount,
                                          FetchSignature = progenitor.FetchSignature,
                                          InferredInnerJoin = progenitor.InferredInnerJoin,
                                          IsFetched = progenitor.IsFetched,
                                          SplitOn = progenitor.SplitOn,
                                          Column = progenitor.Column,
                                          Parent = parent
                                      };

            var clonedChildren = new OrderedDictionary<string, FetchNode>();
            foreach (var keyValue in progenitor.Children) {
                clonedChildren.Add(keyValue.Key, InnerClone(keyValue.Value, clone));
            }

            clone.Children = clonedChildren;
            return clone;
        }
    }
}