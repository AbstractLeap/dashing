namespace Dashing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dashing.Extensions;

    public sealed partial class Session {
        public T Get<T, TPrimaryKey>(TPrimaryKey id)
            where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.MaybeOpenConnection(), this.GetTransaction(), id);
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids)
            where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.MaybeOpenConnection(), this.GetTransaction(), ids);
        }

        public ISelectQuery<T> Query<T>()
            where T : class, new() {
            return new SelectQuery<T>(this);
        }

        public IEnumerable<T> Query<T>(SelectQuery<T> query)
            where T : class, new() {
            return this.engine.Query(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query)
            where T : class, new() {
            return this.engine.QueryPaged(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public int Count<T>(SelectQuery<T> query)
            where T : class, new() {
            return this.engine.Count(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public int Insert<T>(T entities) {
            var underlyingType = typeof(T).GetEnumerableType();

            if (underlyingType != null) { // is T[] or IEnumerable<T>
                return this.InsertFor(underlyingType, (IEnumerable)entities);
            }

            return this.InsertFor(typeof(T), new[] { entities });
        }

        private int InsertFor<T>(Type type, T entities)
            where T : IEnumerable {
            if (InsertMethodsOfType.TryGetValue(type, out var method)) {
                return method(this, entities);
            }

            var insertMethod = typeof(Session).GetMethod(nameof(Insert), BindingFlags.NonPublic | BindingFlags.Instance)
                                              .MakeGenericMethod(type);
            var action = insertMethod.ConvertToStrongDelegate<IEnumerable, int>();

            InsertMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private int Insert<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            int insertedRows = this.engine.Insert(this.MaybeOpenConnection(), this.GetTransaction(), entities);

            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public int Save<T>(T entities) {
            var underlyingType = typeof(T).GetEnumerableType();

            if (underlyingType != null) { // is T[] or IEnumerable<T>
                return this.SaveFor(underlyingType, (IEnumerable)entities);
            }

            return this.SaveFor(typeof(T), new[] { entities });
        }

        private int SaveFor<T>(Type type, T entities)
            where T : IEnumerable {
            if (SaveMethodsOfType.TryGetValue(type, out var method)) {
                return method(this, entities);
            }

            var saveMethod = typeof(Session).GetMethod(nameof(Save), BindingFlags.NonPublic | BindingFlags.Instance)
                                            .MakeGenericMethod(type);
            var action = saveMethod.ConvertToStrongDelegate<IEnumerable, int>();

            SaveMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private int Save<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            int updatedRows = this.engine.Save(this.MaybeOpenConnection(), this.GetTransaction(), entities);

            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            if (predicates == null || !predicates.Any()) {
                throw new ArgumentException("You must provide at least 1 predicate to Update. If you wish to update all entities use UpdateAll");
            }

            return this.engine.Execute(this.MaybeOpenConnection(), this.GetTransaction(), update, predicates);
        }

        public int Delete<T>(T entities) {
            var underlyingType = typeof(T).GetEnumerableType();

            if (underlyingType != null) { // is T[] or IEnumerable<T>
                return this.DeleteFor(underlyingType, (IEnumerable)entities);
            }

            return this.DeleteFor(typeof(T), new[] { entities });
        }

        private int DeleteFor<T>(Type type, T entities)
            where T : IEnumerable {
            if (DeleteMethodsOfType.TryGetValue(type, out var method)) {
                return method(this, entities);
            }

            var deleteMethod = typeof(Session).GetMethod(nameof(Delete), BindingFlags.NonPublic | BindingFlags.Instance)
                                              .MakeGenericMethod(type);
            var action = deleteMethod.ConvertToStrongDelegate<IEnumerable, int>();

            DeleteMethodsOfType.TryAdd(type, action);

            return action(this, entities);
        }

        private int Delete<T>(IEnumerable<T> entities)
            where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = this.engine.Delete(this.MaybeOpenConnection(), this.GetTransaction(), entities);

            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            return this.engine.ExecuteBulkDelete(this.MaybeOpenConnection(), this.GetTransaction(), predicates);
        }

        public int UpdateAll<T>(Action<T> update)
            where T : class, new() {
            return this.engine.Execute(this.MaybeOpenConnection(), this.GetTransaction(), update, null);
        }

        public int DeleteAll<T>()
            where T : class, new() {
            return this.engine.ExecuteBulkDelete<T>(this.MaybeOpenConnection(), this.GetTransaction(), null);
        }
    }
}