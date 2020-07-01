namespace Dashing.Engine.DML {
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;

    internal partial class SelectWriter {
        private static readonly ConcurrentDictionary<WriterQueryCacheKey, string> SingleQueryCache = new ConcurrentDictionary<WriterQueryCacheKey, string>();

        private static readonly ConcurrentDictionary<WriterQueryCacheKey, string> MultipleQueryCache = new ConcurrentDictionary<WriterQueryCacheKey, string>();

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(TPrimaryKey id) {
            return new SqlWriterResult(
                SingleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(false)),
                new DynamicParameters(
                    new {
                            Id = id
                        }));
        }

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            var primaryKeys = ids as TPrimaryKey[] ?? ids.ToArray();

            if (primaryKeys.Count() == 1) {
                return new SqlWriterResult(
                    SingleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(false)),
                    new DynamicParameters(
                        new {
                                Id = primaryKeys.Single()
                            }));
            }

            return new SqlWriterResult(
                MultipleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(true)),
                new DynamicParameters(
                    new {
                            Ids = primaryKeys
                        }));
        }

        private string GenerateGetSql<T>(bool isMultiple) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder("select ");

            foreach (var column in map.OwnedColumns()) {
                this.AddColumn(sql, column);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append(" from ");
            this.Dialect.AppendQuotedTableName(sql, map);

            sql.Append(" where ");
            sql.Append(map.PrimaryKey.Name);
            sql.Append(
                isMultiple
                    ? " in @Ids"
                    : " = @Id");

            return sql.ToString();
        }
    }
}