namespace Dashing.Engine.DML {
    using System.Linq;
    using System.Text;

    internal partial class SelectWriter {
        private QueryTree GenerateNoPagingSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, QueryTree rootQueryNode, StringBuilder sql, int numberCollectionFetches, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, bool isProjectedQuery)
            where T : class, new() {
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            var whereSql = new StringBuilder();
            var orderSql = new StringBuilder();
            if (rootQueryNode == null && enforceAlias) {
                rootQueryNode = new QueryTree(isProjectedQuery, selectQuery.FetchAllProperties, this.Configuration.GetMap<T>());
            }

            // add where clause
            this.AddWhereClause(selectQuery.WhereClauses, whereSql, parameters, aliasProvider, ref rootQueryNode);

            // add select columns
            if (!this.inExistsContext) {
                this.AddRootColumns(selectQuery, columnSql, rootQueryNode, aliasProvider); // do columns second as we may not be fetching but need joins for the where clause
            }
            else {
                columnSql.Append("1");
            }

            // add in the tables
            this.AddTables(selectQuery, tableSql, columnSql, rootQueryNode, aliasProvider, isProjectedQuery);

            // add order by
            if (selectQuery.OrderClauses.Any()) {
                var containsPrimaryKeyClause = this.AddOrderByClause(selectQuery.OrderClauses, orderSql, rootQueryNode, aliasProvider);
                if (numberCollectionFetches > 0 && !containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(rootQueryNode, orderSql, aliasProvider, isFirstOrderClause: false);
                }
            }
            else if (numberCollectionFetches > 0 || selectQuery.SkipN > 0 || selectQuery.TakeN > 0) {
                // need to add a default order on the sort clause
                this.AppendDefaultOrderBy<T>(rootQueryNode, orderSql, aliasProvider);
            }

            // construct the query
            sql.Append("select ");
            sql.Append(columnSql);
            sql.Append(tableSql);
            sql.Append(whereSql);
            sql.Append(orderSql);
            //// if anything is added after orderSql then the paging will probably need changing

            // apply paging
            // only add paging to the query if it doesn't have any collection fetches
            if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                this.Dialect.ApplySkipTake(sql, orderSql, selectQuery.TakeN, selectQuery.SkipN);
                if (selectQuery.TakeN > 0) {
                    parameters.Add("@take", selectQuery.TakeN);
                }

                if (selectQuery.SkipN > 0) {
                    parameters.Add("@skip", selectQuery.SkipN);
                }
            }

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateOnQueryFinish(sql);
            }

            return rootQueryNode;
        }
    }
}