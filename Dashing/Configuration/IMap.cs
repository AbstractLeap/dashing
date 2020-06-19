namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;

    public interface IMap {
        /// <summary>
        ///     Gets the type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Gets the Configuration
        /// </summary>
        IConfiguration Configuration { get; set; }

        /// <summary>
        /// Indicates if the type is owned by other types
        /// This means that the type will not be persisted itself but always inside other types
        /// </summary>
        bool IsOwned { get; set; }

        /// <summary>
        ///     Gets or sets the table.
        /// </summary>
        string Table { get; set; }

        /// <summary>
        ///     Gets or sets the name of the history table if the Type is versioned
        /// </summary>
        string HistoryTable { get; set; }

        /// <summary>
        ///     Gets or sets the schema.
        /// </summary>
        string Schema { get; set; }

        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        IColumn PrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        IDictionary<string, IColumn> Columns { get; }

        /// <summary>
        ///     Gets or sets the indexes specified for this map
        /// </summary>
        IEnumerable<Index> Indexes { get; set; }

        /// <summary>
        ///     Add a new index to the map
        /// </summary>
        /// <param name="index"></param>
        void AddIndex(Index index);

        /// <summary>
        ///     Gets or sets the foreign keys specified for this map
        /// </summary>
        IEnumerable<ForeignKey> ForeignKeys { get; set; }

        /// <summary>
        ///     Gets the primary key for the object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        object GetPrimaryKeyValue(object entity);
    }
}