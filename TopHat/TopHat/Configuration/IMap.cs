namespace TopHat.Configuration {
  using System;
  using System.Collections.Generic;

  public interface IMap {
    /// <summary>
    ///   Gets the type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///   Gets or sets the table.
    /// </summary>
    string Table { get; set; }

    /// <summary>
    ///   Gets or sets the schema.
    /// </summary>
    string Schema { get; set; }

    /// <summary>
    ///   Gets or sets the primary key.
    /// </summary>
    string PrimaryKey { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether is primary key database generated.
    /// </summary>
    bool IsPrimaryKeyDatabaseGenerated { get; set; }

    /// <summary>
    ///   Gets or sets the columns.
    /// </summary>
    IDictionary<string, IColumn> Columns { get; set; }
  }
}