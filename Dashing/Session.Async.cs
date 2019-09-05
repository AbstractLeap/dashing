namespace Dashing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dashing.Extensions;
    using Dashing.Logging;

    using Poly.Logging.Extensions;

    public sealed partial class Session
    {
        public async Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "QueryAsync", id.ToString()))
            {
                return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), id);
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "GetAsync", ids.ToString()))
            {
                return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), ids);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "QueryAsync", query.ToString()))
            {
                return await this.engine.QueryAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
            }
        }

        public async Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "QueryPagedAsync", query.ToString()))
            {
                return await this.engine.QueryPagedAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
            }
        }

        public async Task<int> CountAsync<T>(SelectQuery<T> query) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "CountAsync", query.ToString()))
            {
                return await this.engine.CountAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
            }
        }

        public Task<int> InsertAsync<T>(T entities)
        {
            var underlyingType = typeof(T).GetEnumerableType();

            if (underlyingType != null)
            { // is T[] or IEnumerable<T>
                return this.InsertAsyncFor(underlyingType, (IEnumerable)entities);
            }

            return this.InsertAsyncFor(typeof(T), new[] { entities });
        }

        private Task<int> InsertAsyncFor<T>(Type type, T entities) where T : IEnumerable
        {
            if (InsertAsyncMethodsOfType.TryGetValue(type, out var method))
            {
                return method(this, entities);
            }

            var insertAsyncMethod = typeof(Session).GetMethod(nameof(Session.InsertAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type);
            var action = insertAsyncMethod.ConvertToStrongDelegate<IEnumerable, Task<int>>();

            InsertAsyncMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private async Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners)
                    {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            int insertedRows;

            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "InsertAsync"))
            {
                insertedRows = await this.engine.InsertAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            }

            if (this.Configuration.EventHandlers.PostInsertListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners)
                    {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public Task<int> SaveAsync<T>(T entities)
        {
            var underlyingType = typeof(T).GetEnumerableType();
            if (underlyingType != null)
            { // is T[] or IEnumerable<T>
                return this.SaveAsyncFor(underlyingType, (IEnumerable)entities);
            }

            return this.SaveAsyncFor(typeof(T), new[] { entities });
        }

        private Task<int> SaveAsyncFor<T>(Type type, T entities) where T : IEnumerable
        {
            if (SaveAsyncMethodsOfType.TryGetValue(type, out var method))
            {
                return method(this, entities);
            }

            var saveAsyncMethod = typeof(Session).GetMethod(nameof(Session.SaveAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type);
            var action = saveAsyncMethod.ConvertToStrongDelegate<IEnumerable, Task<int>>();

            SaveAsyncMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private async Task<int> SaveAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners)
                    {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            int updatedRows;

            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "SaveAsync"))
            {
                updatedRows = await this.engine.SaveAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
                this.Configuration.Logger?.TrackMetric("SaveAsync.Rows", updatedRows);
            }

            if (this.Configuration.EventHandlers.PostSaveListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners)
                    {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public async Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new()
        {
            if (predicates == null || !predicates.Any())
            {
                throw new ArgumentException("You must provide at least 1 predicate to Update. If you wish to update all entities use UpdateAll");
            }

            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "UpdateAsync"))
            {
                return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, predicates);
            }
        }

        public Task<int> DeleteAsync<T>(T entities)
        {
            var underlyingType = typeof(T).GetEnumerableType();

            if (underlyingType != null)
            { // is T[] or IEnumerable<T>
                return this.DeleteAsyncFor(underlyingType, (IEnumerable)entities);
            }

            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "DeleteAsync"))
            {
                return this.DeleteAsyncFor(typeof(T), new[] { entities });
            }
        }

        private Task<int> DeleteAsyncFor<T>(Type type, T entities) where T : IEnumerable
        {
            if (DeleteAsyncMethodsOfType.TryGetValue(type, out var method))
            {
                return method(this, entities);
            }

            var deleteAsyncMethod = typeof(Session).GetMethod(nameof(Session.DeleteAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type);
            var action = deleteAsyncMethod.ConvertToStrongDelegate<IEnumerable, Task<int>>();

            DeleteAsyncMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private async Task<int> DeleteAsync<T>(IEnumerable<T> entities)
            where T : class, new()
        {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners)
                    {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            int deletedRows;

            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "DeleteAsync"))
            {
                deletedRows = await this.engine.DeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
                this.Configuration.Logger?.TrackMetric("DeleteAsync.Rows", deletedRows);
            }

            if (this.Configuration.EventHandlers.PostDeleteListeners.Any())
            {
                foreach (var entity in entities)
                {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners)
                    {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "DeleteAsync"))
            {
                return await this.engine.ExecuteBulkDeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), predicates);
            }
        }

        public async Task<int> UpdateAllAsync<T>(Action<T> update) where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "UpdateAllAsync"))
            {
                return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, null);
            }
        }

        public async Task<int> DeleteAllAsync<T>() where T : class, new()
        {
            using (this.Configuration.Logger?.CreateDependencyExecutionTimer(LoggingConfig.DashingDependencyTypeName, "DeleteAllAsync"))
            {
                return await this.engine.ExecuteBulkDeleteAsync<T>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), null);
            }
        }
    }
}