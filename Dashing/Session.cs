namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine;

    public sealed class Session : ISession, ISelectQueryExecutor {
        public IDapper Dapper { get; private set; }

        private readonly IEngine engine;

        private readonly ISessionState sessionState;

        public Session(IEngine engine, ISessionState sessionState) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            this.engine = engine;
            this.sessionState = sessionState;
            this.Dapper = new DapperWrapper(sessionState);
        }

        public IConfiguration Configuration
        {
            get
            {
                return this.engine.Configuration;
            }
        }

        public void Dispose() {
            this.sessionState.Dispose();
        }

        public void Complete() {
            this.sessionState.Complete();
        }

        public void Reject() {
            this.sessionState.Reject();
        }

        public T Get<T, TPrimaryKey>(TPrimaryKey id) where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.sessionState, id);
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.sessionState, ids);
        }

        public ISelectQuery<T> Query<T>() where T : class, new() {
            return new SelectQuery<T>(this);
        }

        public IEnumerable<T> Query<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.Query(this.sessionState, query);
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.QueryPaged(this.sessionState, query);
        }

        public int Count<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.Count(this.sessionState, query);
        }

        public int Insert<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = this.engine.Insert(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public int Save<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = this.engine.Save(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return this.engine.Execute(this.sessionState, update, predicates);
        }

        public int Delete<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = this.engine.Delete(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return this.engine.ExecuteBulkDelete(this.sessionState, predicates);
        }

        public int UpdateAll<T>(Action<T> update) where T : class, new() {
            return this.engine.Execute(this.sessionState, update, null);
        }

        public int DeleteAll<T>() where T : class, new() {
            return this.engine.ExecuteBulkDelete<T>(this.sessionState, null);
        }

        public async Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id) where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(this.sessionState, id);
        }

        public async Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(this.sessionState, ids);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.QueryAsync(this.sessionState, query);
        }

        public async Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.QueryPagedAsync(this.sessionState, query);
        }

        public async Task<int> CountAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.CountAsync(this.sessionState, query);
        }

        public async Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = await this.engine.InsertAsync(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public async Task<int> SaveAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = await this.engine.SaveAsync(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public async Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return await this.engine.ExecuteAsync(this.sessionState, update, predicates);
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = await this.engine.DeleteAsync(this.sessionState, entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync(this.sessionState, predicates);
        }

        public async Task<int> UpdateAllAsync<T>(Action<T> update) where T : class, new() {
            return await this.engine.ExecuteAsync(this.sessionState, update, null);
        }

        public async Task<int> DeleteAllAsync<T>() where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync<T>(this.sessionState, null);
        }
    }
}