namespace Dashing.Engine.InMemory {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if NETSTANDARD2_0
    using System.Reflection;
#endif

    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Extensions;

    public class FetchCloner {
        private readonly IConfiguration configuration;

        private readonly FetchTreeParser fetchTreeParser;

        public FetchCloner(IConfiguration configuration) {
            this.configuration = configuration;
            this.fetchTreeParser = new FetchTreeParser(configuration);
        }

        public T Clone<T>(SelectQuery<T> query, T entity) where T : class, new() {
            var result = new T();
            int aliasCounter;
            int numberCollectionFetches;
            var fetchTree = this.fetchTreeParser.GetFetchTree(query, out aliasCounter, out numberCollectionFetches);
            this.Clone(entity, result, fetchTree);
            return result;
        }

        private void Clone(object entity, object result, FetchNode fetchNode) {
            var entityType = entity.GetType();
            foreach (var column in Enumerable.Where<KeyValuePair<string, IColumn>>(this.configuration.GetMap(entityType).Columns, c => !c.Value.IsIgnored)) {
                var prop = entityType.GetProperty(column.Key);
                if (column.Value.Type.IsValueType()) {
                    prop.SetValue(result, prop.GetValue(entity));
                }
                else if (column.Value.Type == typeof(string)) {
                    var val = prop.GetValue(entity) as string;
                    if (val != null) {
                        val = new string(val.ToCharArray());
                    }

                    prop.SetValue(result, val);
                }
                else if (column.Value.Relationship == RelationshipType.ManyToOne || column.Value.Relationship == RelationshipType.OneToOne) {
                    var val = prop.GetValue(entity);
                    if (val != null) {
                        if (fetchNode != null && fetchNode.Children.ContainsKey(column.Key)) {
                            // fetched, we need to deep clone this
                            var fetchedResult = Activator.CreateInstance(column.Value.Type);
                            this.Clone(val, fetchedResult, fetchNode.Children[column.Key]);
                            prop.SetValue(result, fetchedResult);
                        }
                        else {
                            // if not fetched, then this is null but set the backing field value if necessary
                            var map = column.Value.Relationship == RelationshipType.ManyToOne
                                          ? column.Value.ParentMap
                                          : column.Value.OppositeColumn.Map;
                            var primaryKey = map.GetPrimaryKeyValue(val);
                            var field = entityType.GetField(column.Value.DbName);
                            field.SetValue(result, primaryKey);
                        }
                    }
                    else {
                        if (!column.Value.IsNullable) {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The property {0} on {1} is marked as not nullable. You must add some data for it",
                                    column.Key,
                                    result.GetType()));
                        }
                    }
                }
                else if (column.Value.Relationship == RelationshipType.OneToMany) {
                    var val = prop.GetValue(entity) as ICollection;
                    if (val != null && val.Count > 0) {
                        var listEntityType = column.Value.ChildColumn.Map.Type;
                        var listType = typeof(List<>).MakeGenericType(listEntityType);
                        var listResult = Activator.CreateInstance(listType);
                        foreach (var collectionEntity in val) {
                            var collectionResult = Activator.CreateInstance(listEntityType);
                            this.Clone(
                                collectionEntity,
                                collectionResult,
                                fetchNode != null && fetchNode.Children.ContainsKey(column.Key) ? fetchNode.Children[column.Key] : null);
                            listType.GetMethod("Add").Invoke(listResult, new[] { collectionResult });
                        }
                        prop.SetValue(result, listResult);
                    }
                }
            }
        }
    }
}