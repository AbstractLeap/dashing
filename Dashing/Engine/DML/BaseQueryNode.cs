namespace Dashing.Engine.DML {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public abstract class BaseQueryNode
    {
        protected QueryTree tree;

        public bool IsFetched { get; protected set; }

        protected int containedCollectionFetchesCount;

        public int ContainedCollectionFetchesCount => this.containedCollectionFetchesCount;

        public bool InferredInnerJoin { get; set; }

        public OrderedDictionary<string, QueryNode> Children { get; set; }

        public BaseQueryNode() {
            this.Children = new OrderedDictionary<string, QueryNode>();
        }

        public QueryNode AddChild(IColumn column, bool isFetched)
        {
            // create the queryNode
            var newNode = new QueryNode(this, this.tree, column, isFetched, false);
            if (column.Relationship == RelationshipType.OneToMany)
            {
                // go through and increase the number of contained collections in each parent queryNode
                var parent = this;
                while (parent != null)
                {
                    ++parent.containedCollectionFetchesCount;
                    if (parent is QueryNode mapQueryNode) {
                        parent = mapQueryNode.Parent;
                    } else {
                        parent = null;
                    }
                }
            }

            // insert it
            if (this.Children.Any())
            {
                var i = 0;
                var inserted = false;
                foreach (var child in this.Children)
                {
                    if (child.Value.Column.FetchId > newNode.Column.FetchId)
                    {
                        this.Children.Insert(i, new KeyValuePair<string, QueryNode>(column.Name, newNode));
                        inserted = true;
                        break;
                    }

                    ++i;
                }

                if (!inserted) this.Children.Add(column.Name, newNode);
            }
            else
            {
                this.Children.Add(column.Name, newNode);
            }

            return newNode;
        }

        private IList<IColumn> includedColumns;

        private IList<IColumn> excludedColumns;

        public void AddIncludedColumn(IColumn column)
        {
            if (this.includedColumns == null)
            {
                this.includedColumns = new List<IColumn>();
            }

            this.includedColumns.Add(column);
        }

        public void AddExcludedColumn(IColumn column)
        {
            if (this.excludedColumns == null)
            {
                this.excludedColumns = new List<IColumn>();
            }

            this.excludedColumns.Add(column);
        }

        public IEnumerable<IColumn> GetSelectedColumns()
        {
            if (!this.IsFetched)
            {
                return Enumerable.Empty<IColumn>();
            }

            if (this.tree.IsProjected)
            {
                if (this.includedColumns != null)
                {
                    return ExcludeFetchedForeignKeys(this.includedColumns);
                }

                // TODO cope with root node (need to add the map somehow
                return ExcludeFetchedForeignKeys(GetOwnedColumns());
            }

            var columns = GetOwnedColumns();
            if (this.includedColumns != null)
            {
                columns = columns.Union(this.includedColumns);
            }

            if (this.excludedColumns != null)
            {
                columns = columns.Where(c => !this.excludedColumns.Contains(c));
            }

            return ExcludeFetchedForeignKeys(columns);

            IEnumerable<IColumn> ExcludeFetchedForeignKeys(IEnumerable<IColumn> returnColumns) {
                return returnColumns.Where(c => !this.Children.TryGetValue(c.Name, out var child) || !child.IsFetched);
            }

            IEnumerable<IColumn> GetOwnedColumns()
            {
                return this.GetMapForNode().OwnedColumns(this.tree.FetchAllProperties);
            }
        }

        public abstract IMap GetMapForNode();
    }
}