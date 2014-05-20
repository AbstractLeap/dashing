﻿namespace TopHat.Configuration {
  using System;
  using System.Data;

  public interface IColumn {
    /// <summary>
    ///   Gets the type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///   Gets or sets the db type.
    /// </summary>
    DbType DbType { get; set; }

    /// <summary>
    ///   Gets or sets the database field name.
    /// </summary>
    string DbName { get; set; }

    /// <summary>
    ///   Gets or sets the precision.
    /// </summary>
    byte Precision { get; set; }

    /// <summary>
    ///   Gets or sets the scale.
    /// </summary>
    byte Scale { get; set; }

    /// <summary>
    ///   Gets or sets the length.
    /// </summary>
    ushort Length { get; set; }

    /// <summary>
    ///   Indicates whether the column will be ignored for all queries and schema generation
    /// </summary>
    bool Ignore { get; set; }

    /// <summary>
    ///   Indicates whether the column will be excluded from select queries unless specifically requested
    /// </summary>
    bool ExcludeByDefault { get; set; }

    /// <summary>
    ///   Gets or sets the relationship.
    /// </summary>
    RelationshipType Relationship { get; set; }
  }
}