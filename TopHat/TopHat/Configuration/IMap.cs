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
    }

    public interface IMap<T> : IMap
    {
    }
}