namespace Dashing.Engine.DML {
    using System;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public class QueryTree : BaseQueryNode
    {
        public QueryTree(bool isProjected, bool fetchAllProperties, IMap rootMap) {
            this.IsProjected = isProjected;
            this.FetchAllProperties = fetchAllProperties;
            this.RootMap = rootMap;
            this.IsFetched = true;
            this.tree = this;
        }

        public IMap RootMap { get; }

        public bool FetchAllProperties { get; }

        public bool IsProjected { get; }

        public string GetFetchSignature() {
            return string.Join("", this.Children.Values.Select(this.GetFetchSignature));
        }

        private string GetFetchSignature(QueryNode node) {
            var innerSignature = string.Join("", node.Children.Select(c => this.GetFetchSignature(c.Value)));
            return node.IsFetched
                       ? $"{node.Column.FetchId}S{innerSignature}E"
                       : innerSignature;
        }

        public string GetSplitOn() {
            return string.Join(",", this.Children.Select(c => this.GetSplitOn(c.Value)).Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private string GetSplitOn(QueryNode node) {
            if (!node.IsFetched) {
                return string.Empty;
            }

            var mySplit = node.GetSelectedColumns()
                              .First()
                              .Name;
            var childSplit = string.Join(",", node.Children.Select(n => this.GetSplitOn(n.Value)).Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(childSplit)) {
                return $"{mySplit},{childSplit}";
            }

            return mySplit;
        }

        public override IMap GetMapForNode() {
            return this.RootMap;
        }

        public QueryTree Clone()
        {
            var clone = new QueryTree(this.IsProjected, this.FetchAllProperties, this.RootMap) {
                                                                                                   containedCollectionFetchesCount = this.containedCollectionFetchesCount
                                                                                               };

            var clonedChildren = new OrderedDictionary<string, QueryNode>();
            foreach (var child in this.Children) {
                clonedChildren.Add(child.Key, child.Value.Clone(clone, clone));
            }

            clone.Children = clonedChildren;
            return clone;
        }
    }
}