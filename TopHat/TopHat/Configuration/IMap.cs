using System;
using System.Collections.Generic;

namespace TopHat.Configuration
{
    public interface IMap
    {
        Type Type { get; set; }

        string Table { get; set; }

        string Schema { get; set; }

        string PrimaryKey { get; set; }

        bool IsPrimaryKeyDatabaseGenerated { get; set; }

        IList<Column> Columns { get; set; }

        IList<IList<string>> Indexes { get; set; }

        /// <summary>
        /// The query to execute in order to select by primary key
        /// </summary>
        string SqlSelectByPrimaryKey { get; }

        /// <summary>
        /// The query to execute in order to select all columns by primary key
        /// </summary>
        string SqlSelectByPrimaryKeyIncludeAllColumns { get; }

        /// <summary>
        /// The query to execute in order to insert an entity
        /// </summary>
        string SqlInsert { get; }

        /// <summary>
        /// The query to execute in order to delete by primary key
        /// </summary>
        string SqlDeleteByPrimaryKey { get; }
    }

    public interface IMap<T> : IMap
    {
    }
}