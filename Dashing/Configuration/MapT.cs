namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    ///     The map.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class Map<T> : Map, IMap<T> {
        public Map()
            : base(typeof(T)) { }

        private readonly object primaryKeyGetSetLock = new object();

        private readonly object propertyGettersLock = new object();

        private Func<T, object> primaryKeyGetter;

        private Action<T, object> primaryKeySetter;

        private IDictionary<IColumn, Func<T, object>> propertyGetters;

        public object GetPrimaryKeyValue(T entity) {
            if (this.primaryKeyGetter == null) {
                lock (this.primaryKeyGetSetLock) {
                    if (this.primaryKeyGetter == null) {
                        if (this.PrimaryKey == null) {
                            throw new Exception("Primary Key is null on the Map");
                        }

                        var param = Expression.Parameter(typeof(T));
                        this.primaryKeyGetter = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, this.PrimaryKey.Name), typeof(object)), param)
                                                          .Compile();
                    }
                }
            }

            return this.primaryKeyGetter(entity);
        }

        public void SetPrimaryKeyValue(T entity, object value) {
            if (this.primaryKeySetter == null) {
                lock (this.primaryKeyGetSetLock) {
                    if (this.primaryKeySetter == null) {
                        if (this.PrimaryKey == null) {
                            throw new Exception("Primary Key is null on the Map");
                        }

                        var param = Expression.Parameter(typeof(T));
                        var valueParam = Expression.Parameter(typeof(object));
                        this.primaryKeySetter =
                            Expression.Lambda<Action<T, object>>(
                                Expression.Assign(Expression.Property(param, this.PrimaryKey.Name), Expression.Convert(valueParam, typeof(int))),
                                new[] { param, valueParam })
                                      .Compile();
                    }
                }
            }

            this.primaryKeySetter(entity, value);
        }

        public object GetColumnValue(T entity, IColumn column) {
            if (this.propertyGetters == null) {
                lock (this.propertyGettersLock) {
                    if (this.propertyGetters == null) {
                        this.propertyGetters = new Dictionary<IColumn, Func<T, object>>();
                        foreach (var col in this.Columns) {
                            var param = Expression.Parameter(typeof(T));
                            var getter = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, col.Key), typeof(object)), param)
                                                   .Compile();
                            this.propertyGetters.Add(col.Value, getter);
                        }
                    }
                }
            }

            return this.propertyGetters[column](entity);
        }

        /// <summary>
        ///     The from.
        /// </summary>
        /// <param name="map">
        ///     The map.
        /// </param>
        /// <remarks>
        ///     Highly inelegant wrapping of all the members, but probably quite performant
        /// </remarks>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
        public static Map<T> From(IMap map) {
            var genericMap = map as Map<T>;
            if (genericMap != null) {
                return genericMap;
            }

            if (typeof(T) != map.Type) {
                throw new ArgumentException("The argument does not represent a map of the correct generic type");
            }

            return new Map<T> {
                Table = map.Table,
                Schema = map.Schema,
                PrimaryKey = map.PrimaryKey,
                Columns = map.Columns,
                Configuration = map.Configuration

                //// Indexes = map.Indexes
            };
        }
    }
}