namespace TopHat.Configuration {
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The map.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class Map<T> : IMap {
        public Map() {
            this.Type = typeof(T);
            this.Columns = new Dictionary<string, IColumn>();

            //// this.Indexes = new List<IEnumerable<string>>();
        }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        ///     Gets or sets the table.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        ///     Gets or sets the schema.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        public IColumn PrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        public IDictionary<string, IColumn> Columns { get; set; }

        //// commented out until we get basic stuff working
        ///// <summary>
        /////   Gets or sets the indexes.
        ///// </summary>
        //// public IEnumerable<IEnumerable<string>> Indexes { get; set; }

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
            if (typeof(T) != map.Type) {
                throw new ArgumentException("The argument does not represent a map of the correct generic type");
            }

            return new Map<T> {
                                  Table = map.Table, 
                                  Schema = map.Schema, 
                                  PrimaryKey = map.PrimaryKey, 
                                  Columns = map.Columns
                          
                                  //// Indexes = map.Indexes
                              };
        }
    }
}