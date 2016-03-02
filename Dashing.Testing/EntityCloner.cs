namespace Dashing.Testing {
    using System;

    using Dashing.Configuration;

    public class EntityCloner<TEntity>
        where TEntity : new() {
        private readonly IConfiguration configuration;

        private readonly Type entityType;

        public EntityCloner(IConfiguration configuration) {
            this.configuration = configuration;
            this.entityType = typeof(TEntity);
        }

        public TEntity Clone(TEntity entity) {
            var result = new TEntity();
            foreach (var column in this.configuration.GetMap<TEntity>().OwnedColumns(true)) {
                var prop = this.entityType.GetProperty(column.Name);
                if (column.Type.IsValueType) {
                    prop.SetValue(result, prop.GetValue(entity));
                }
                else if (column.Type == typeof(string)) {
                    var val = prop.GetValue(entity) as string;
                    if (val != null) {
                        val = string.Copy(val);
                    }

                    prop.SetValue(result, val);
                }
                else if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                    // all we want here is to clone the entity and just leave the primary key on
                    var val = prop.GetValue(entity);
                    if (val != null) {
                        var map = column.Relationship == RelationshipType.ManyToOne ? column.ParentMap : column.OppositeColumn.Map;
                        var primaryKey = map.GetPrimaryKeyValue(val);
                        var field = this.entityType.GetField(column.DbName);
                        field.SetValue(result, primaryKey);
                    }
                    else {
                        if (!column.IsNullable) {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The property {0} on {1} is marked as not nullable. You must add some data for it",
                                    column.Name,
                                    result.GetType()));
                        }
                    }
                }
            }

            return result;
        }
    }
}