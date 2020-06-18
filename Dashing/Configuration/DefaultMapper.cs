namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using Dashing.Extensions;
    using Dashing.Versioning;

    /// <summary>
    ///     The default mapper.
    /// </summary>
    public class DefaultMapper : IMapper {
        /// <summary>
        ///     The _convention.
        /// </summary>
        private readonly IConvention convention;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultMapper" /> class.
        /// </summary>
        /// <param name="convention">
        ///     The convention.
        /// </param>
        public DefaultMapper(IConvention convention) {
            if (convention == null) {
                throw new ArgumentNullException("convention");
            }

            this.convention = convention;
        }

        /// <summary>
        ///     Return a typed map for the typeparameter specified
        /// </summary>
        /// <typeparam name="T">Type to be mapped</typeparam>
        /// <param name="configuration">Configuration that the map belongs to</param>
        /// <returns>Map for the type</returns>
        public IMap<T> MapFor<T>(IConfiguration configuration) {
            var map = new Map<T>();
            this.Build(typeof(T), map, configuration);
            return map;
        }

        /// <summary>
        ///     Return a generic map for the type specified
        /// </summary>
        /// <param name="type">Type to be mapped</param>
        /// <param name="configuration">Configuration that the map belongs to</param>
        /// <returns>Map for the type</returns>
        public IMap MapFor(Type type, IConfiguration configuration) {
            var gt = typeof(Map<>).MakeGenericType(type);
            var ctor = gt.GetConstructor(new Type[] { });
            if (ctor == null) {
                throw new Exception("Could not locate constructor for the Map type?");
            }

            var map = (IMap)ctor.Invoke(new object[] { });
            this.Build(type, map, configuration);
            return map;
        }

        private void Build(Type entityType, IMap map, IConfiguration configuration) {
            map.Configuration = configuration;
            map.Table = this.convention.TableFor(entityType);
            map.Schema = this.convention.SchemaFor(entityType);
            if (entityType.IsVersionedEntity()) {
                map.HistoryTable = this.convention.HistoryTableFor(entityType);
            }

            entityType.GetMembers(this.convention.MemberBindingFlags(entityType))
                  .Select(member => this.BuildColumn(map, entityType, member, configuration))
                  .ToList()
                  .ForEach(c => map.Columns.Add(c.Name, c));
            this.ResolvePrimaryKey(entityType, map);
            this.AssignFetchIds(map);
        }

        /// <summary>
        ///     Assigns fetch ids to the non local columns using a consistent strategy (namely column name)
        /// </summary>
        /// <param name="map"></param>
        private void AssignFetchIds(IMap map) {
            int i = 0;
            var columns = map.Columns.Where(c => c.Value.Relationship != RelationshipType.None).OrderBy(c => c.Key);
            foreach (var column in columns) {
                column.Value.FetchId = ++i;
            }
        }

        private IColumn BuildColumn(IMap map, Type entityType, MemberInfo memberInfo, IConfiguration configuration) {
            var memberType = GetMemberType(memberInfo);

            // TODO: this can be cached
            var column = (IColumn)Activator.CreateInstance(typeof(Column<>).MakeGenericType(memberType));
            column.Map = map;
            column.Name = memberInfo.Name;
            column.IsIgnored = this.convention.IsIgnored(entityType, memberInfo);
            column.IsComputed = entityType.IsVersionedEntity() && typeof(IVersionedEntity<>).GetProperties().Select(pi => pi.Name).Contains(memberInfo.Name);

            this.ResolveRelationship(entityType, memberInfo, column, configuration);
            this.ApplyAnnotations(entityType, memberInfo, column);

            return column;
        }

        private static Type GetMemberType(MemberInfo memberInfo) {
            var memberType = (memberInfo as PropertyInfo)?.PropertyType ?? (memberInfo as FieldInfo)?.FieldType;
            if (memberType == null) {
                throw new InvalidOperationException($"Unable to determine type of member {memberInfo}");
            }

            return memberType;
        }

        private void ResolveRelationship(Type entity, MemberInfo memberInfo, IColumn column, IConfiguration configuration) {
            var memberType = GetMemberType(memberInfo);
            if (memberType.IsEntityType()) {
                if (memberType.IsCollection()) {
                    this.ResolveOneToManyColumn(column);
                }
                else {
                    this.ResolveEntityColumn(column, memberInfo.Name, configuration);
                }
            }
            else {
                this.ResolveValueColumn(entity, column, memberInfo.Name, memberType);
            }
        }

        private void ResolveValueColumn(Type entity, IColumn column, string propertyName, Type propertyType) {
            column.Relationship = RelationshipType.None;
            column.DbName = propertyName;
            column.DbType = this.convention.GetDbTypeFor(propertyType);
            column.IsNullable = propertyType.IsNullable();

            // check particular types for defaults
            switch (column.DbType) {
                case DbType.Decimal:
                    column.Precision = this.convention.DecimalPrecisionFor(entity, propertyName);
                    column.Scale = this.convention.DecimalScaleFor(entity, propertyName);
                    break;

                case DbType.String:
                    column.Length = this.convention.StringLengthFor(entity, propertyName);
                    column.IsNullable = true;
                    break;

                case DbType.DateTime2:
                    column.Precision = this.convention.DateTime2PrecisionFor(entity, propertyName);
                    break;
            }
        }

        private void ResolveEntityColumn(IColumn column, string propertyName, IConfiguration configuration) {
            if (configuration.HasMap(column.Type)) {
                // we have a reference to the referenced type
                // note, that at this point (in general) we may have not mapped the opposite type yet
                // find a property with our type
                var candidateColumns =
                    configuration.GetMap(column.Type)
                                 .Columns.Where(
                                     c =>
                                     (c.Value.Type == column.Map.Type
                                      || (c.Value.Type.IsCollection() && c.Value.Type.GetGenericArguments().First() == column.Map.Type))
                                     && !c.Value.IsIgnored)
                                 .ToArray();
                if (candidateColumns.Length == 0) {
                    // assume many to one
                    this.ResolveManyToOneColumn(column, propertyName);
                }
                else if (candidateColumns.Length == 1) {
                    if (candidateColumns[0].Value.Type.IsCollection()) {
                        this.ResolveManyToOneColumn(column, propertyName);
                    }
                    else {
                        // assume one to one
                        this.ResolveOneToOneColumn(column, propertyName);

                        // now fix the other side
                        candidateColumns[0].Value.Relationship = RelationshipType.OneToOne;
                        candidateColumns[0].Value.IsNullable = true; // we match up what ResolveOneToOneColumn does
                    }
                }
                else {
                    // ambiguous column reference, go for many to one and assume it gets sorted by further config later
                    this.ResolveManyToOneColumn(column, propertyName);
                }
            }
            else {
                if (column.Type == column.Map.Type) {
                    // self referencing
                    // at this point not all columns may be mapped so we'll use reflection
                    if (column.Map.Type.GetProperties().Any(p => p.PropertyType == column.Type && p.Name != column.Name)) {
                        this.ResolveOneToOneColumn(column, propertyName);
                    }
                    else {
                        this.ResolveManyToOneColumn(column, propertyName);
                    }
                }
                else {
                    // we don't currently have the other side of the map so assume many to one and rely on the above (when it is mapped) to fix
                    this.ResolveManyToOneColumn(column, propertyName);
                }
            }
        }

        private void ResolveOneToOneColumn(IColumn column, string propertyName) {
            column.Relationship = RelationshipType.OneToOne;
            column.DbName = propertyName + "Id";
            column.IsNullable = true;
        }

        private void ResolveManyToOneColumn(IColumn column, string propertyName) {
            column.Relationship = RelationshipType.ManyToOne;
            column.DbName = propertyName + "Id";
            column.IsNullable = this.convention.IsManyToOneNullable(column.Map.Type, propertyName);
        }

        private void ResolveOneToManyColumn(IColumn column) {
            // assume to be OneToMany
            column.Relationship = RelationshipType.OneToMany;
            column.ShouldWeavingInitialiseListInConstructor = this.convention.IsCollectionInstantiationAutomatic(column.Map.Type, column.Name);
        }

        private void ApplyAnnotations(Type entity, MemberInfo property, IColumn column) {
            /* should do something, innit! */
        }

        private void ResolvePrimaryKey(Type entity, IMap map) {
            // get the name from the convention
            var primaryKeyName = this.convention.PrimaryKeyFor(entity, map.OwnedColumns(true).Select(c => c.Name));
            if (primaryKeyName == null) {
                return;
            }

            // find the property
            map.PrimaryKey = map.Columns.Values.FirstOrDefault(c => c.Name.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase));
            if (map.PrimaryKey == null) {
                return;
            }

            // ensure primary key comes first in columns
            map.Columns.Remove(map.PrimaryKey.Name);
            ((OrderedDictionary<string, IColumn>)map.Columns).Insert(0, new KeyValuePair<string, IColumn>(map.PrimaryKey.Name, map.PrimaryKey));

            // enforce some column properties
            map.PrimaryKey.IsPrimaryKey = true;
            map.PrimaryKey.IsAutoGenerated = this.convention.IsPrimaryKeyAutoGenerated(map.PrimaryKey);
            map.PrimaryKey.IsNullable = false;
        }
    }
}