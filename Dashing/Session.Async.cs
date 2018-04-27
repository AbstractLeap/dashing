namespace Dashing {
    using Dashing.SqlBuilder;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public sealed partial class Session {
        public async Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id)
            where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), id);
        }

        public async Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids)
            where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), ids);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query)
            where T : class, new() {
            return await this.engine.QueryAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query)
            where T : class, new() {
            return await this.engine.QueryPagedAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> CountAsync<T>(SelectQuery<T> query)
            where T : class, new() {
            return await this.engine.CountAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> InsertAsync<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = await this.engine.InsertAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public async Task<int> SaveAsync<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = await this.engine.SaveAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public async Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            if (predicates == null || !predicates.Any()) {
                throw new ArgumentException("You must provide at least 1 predicate to Update. If you wish to update all entities use UpdateAll");
            }

            return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, predicates);
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = await this.engine.DeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), predicates);
        }

        public async Task<int> UpdateAllAsync<T>(Action<T> update)
            where T : class, new() {
            return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, null);
        }

        public async Task<int> DeleteAllAsync<T>()
            where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync<T>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), null);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression) {
            return await this.engine.QueryAsync<T>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), baseSqlFromDefinition, selectExpression);
        }
    }
}