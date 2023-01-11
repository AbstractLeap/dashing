namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;

    using Dashing.Extensions;

    public class DefaultAliasProvider : IAliasProvider {
        private Dictionary<BaseQueryNode, string> aliases;

        private int fetchedAliasCounter = 1;

        private int nonFetchedAliasCounter = 100;

        private readonly string prefix;

        private const string DefaultAlias = "t";

        public DefaultAliasProvider() : this(DefaultAlias) { }

        public DefaultAliasProvider(string prefix) {
            this.prefix = prefix;
        }

        public string GetAlias(BaseQueryNode queryNode)
        {
            // TODO make this far simpler and far more performant. Works like this 
            // to be compatible with all the tests from pre FetchNode -> QueryNode refactor
            if (this.aliases == null) {
                this.aliases = new Dictionary<BaseQueryNode, string>(ReferenceEqualityComparer<BaseQueryNode>.Instance);
                var queryTree = FindRoot(queryNode);
                CollectAliases(queryTree);
            }

            if (!this.aliases.TryGetValue(queryNode, out var alias)) {
                if (queryNode is QueryNode concreteQueryNode) {
                    alias = GetNextAlias(concreteQueryNode);
                    this.aliases.Add(queryNode, alias);
                }
                else {
                    throw new InvalidOperationException();
                }
            }

            return alias;
        }

        private void CollectAliases(QueryTree queryTree) {
            this.aliases.Add(queryTree, this.prefix);
            foreach (var queryTreeChild in queryTree.Children) {
                CollectAliases(queryTreeChild.Value);
            }
        }

        private void CollectAliases(QueryNode queryNode) {
            var alias = this.GetNextAlias(queryNode);
            this.aliases.Add(queryNode, alias);
            foreach (var queryNodeChild in queryNode.Children) {
                CollectAliases(queryNodeChild.Value);
            }
        }

        private string GetNextAlias(QueryNode queryNode) {
            return queryNode.IsFetched
                            ? $"{this.prefix}_{this.fetchedAliasCounter++}"
                            : $"{this.prefix}_{this.nonFetchedAliasCounter++}";
        }

        private QueryTree FindRoot(BaseQueryNode baseQueryNode) {
            if (baseQueryNode is QueryTree queryTree) {
                return queryTree;
            }

            if (baseQueryNode is QueryNode queryNode) {
                return FindRoot(queryNode.Parent);
            }

            throw new NotSupportedException();
        }
    }
}