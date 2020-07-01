namespace Dashing.Engine.DML {
    using Dashing.Configuration;
    using Dashing.Extensions;

    public class QueryNode : BaseQueryNode
    {

        /// <summary>
        /// the proeprty on the map that refers to this mapnode
        /// </summary>
        public IColumn Column { get; private set; }

        public BaseQueryNode Parent { get; }

        public QueryNode(
            BaseQueryNode parent,
            QueryTree tree, 
            IColumn column,
            bool isFetched,
            bool inferredInnerJoin) {
            this.Parent = parent;
            this.tree = tree;
            this.Column = column;
            this.IsFetched = isFetched;
            this.InferredInnerJoin = inferredInnerJoin;
        }

        public override IMap GetMapForNode() {
            return this.Column.GetMapOfColumnType();
        }

        internal QueryNode Clone(BaseQueryNode parent, QueryTree tree) {
            var node = new QueryNode(parent, tree, this.Column, this.IsFetched, this.InferredInnerJoin);
            var children = new OrderedDictionary<string, QueryNode>();
            foreach (var child in this.Children) {
                children.Add(child.Key, child.Value.Clone(node, tree));
            }

            node.Children = children;
            return node;
        }

        public bool HasAnyNullableAncestor() {
            if (this.Column == null) {
                return false;
            }

            if (this.Column.IsNullable && !this.InferredInnerJoin) {
                return true;
            }

            return this.Parent is QueryNode parentNode && parentNode.HasAnyNullableAncestor();
        }
    }
}