namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public class FetchNode {
        private int aliasCounter;

        private int nonFetchedAliasCounter;

        /// <summary>
        ///     Creates a new root node
        /// </summary>
        public FetchNode() {
            this.Children = new OrderedDictionary<string, FetchNode>();
            this.aliasCounter = 0;
            this.nonFetchedAliasCounter = 99;
            this.Alias = "t";
            this.Root = this;
        }

        public IColumn Column { get; private set; }

        public string Alias { get; private set; }

        public OrderedDictionary<string, FetchNode> Children { get; set; }

        public FetchNode Parent { get; private set; }

        public FetchNode Root { get; private set; }

        /// <summary>
        ///     Indicates whether the property here is being fetch or simply used in a where clause
        /// </summary>
        public bool IsFetched { get; private set; }

        /// <summary>
        /// Indicates that the property is owned, so a different instance but the same table
        /// </summary>
        public bool IsOwned { get; private set; }

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

        public IList<IColumn> IncludedColumns { get; set; }

        public IList<IColumn> ExcludedColumns { get; set; }

        public FetchNode AddChild(IColumn column, bool isFetched, bool isOwned = false) {
            // create the node
            var newNode = new FetchNode {
                                            Alias = "t_" + (isFetched
                                                                ? ++this.Root.aliasCounter
                                                                : ++this.Root.nonFetchedAliasCounter),
                                            IsFetched = isFetched,
                                            IsOwned = isOwned,
                                            Parent = this,
                                            Root = this.Root,
                                            Column = column
                                        };

            if (column.Relationship == RelationshipType.OneToMany) {
                // go through and increase the number of contained collections in each parent node
                var parent = this;
                while (parent != null) {
                    ++parent.ContainedCollectionfetchesCount;
                    parent = parent.Parent;
                }
            }

            // insert it
            if (this.Children.Any()) {
                var i = 0;
                var inserted = false;
                foreach (var child in this.Children) {
                    if (child.Value.Column.FetchId > newNode.Column.FetchId) {
                        this.Children.Insert(i, new KeyValuePair<string, FetchNode>(column.Name, newNode));
                        inserted = true;
                        break;
                    }

                    ++i;
                }

                if (!inserted) this.Children.Add(column.Name, newNode);
            }
            else {
                this.Children.Add(column.Name, newNode);
            }

            return newNode;
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
            clone.Root = parent?.Root ?? clone;

            var clonedChildren = new OrderedDictionary<string, FetchNode>();
            foreach (var keyValue in progenitor.Children) {
                clonedChildren.Add(keyValue.Key, InnerClone(keyValue.Value, clone));
            }

            clone.Children = clonedChildren;
            return clone;
        }
    }
}