namespace TopHat.Configuration {
  using System;
  using System.Data;

  /// <summary>
  ///   The column.
  /// </summary>
  /// <typeparam name="T">
  /// </typeparam>
  public class Column<T> : IColumn {
    public Column() {
      this.Type = typeof(T);
    }

    /// <summary>
    ///   Gets the type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Gets or sets the db type.
    /// </summary>
    public DbType DbType { get; set; }

    /// <summary>
    ///   Gets or sets the db name.
    /// </summary>
    public string DbName { get; set; }

    /// <summary>
    ///   Gets or sets the precision.
    /// </summary>
    public byte Precision { get; set; }

    /// <summary>
    ///   Gets or sets the scale.
    /// </summary>
    public byte Scale { get; set; }

    /// <summary>
    ///   Gets or sets the length.
    /// </summary>
    public ushort Length { get; set; }

    /// <summary>
    ///   Indicates whether the column will be ignored for all queries and schema generation
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    ///   Indicates whether the column will be excluded from select queries unless specifically requested
    /// </summary>
    public bool ExcludeByDefault { get; set; }

    /// <summary>
    ///   Gets or sets the relationship.
    /// </summary>
    public RelationshipType Relationship { get; set; }

    /// <summary>
    ///   The from.
    /// </summary>
    /// <param name="column">
    ///   The column.
    /// </param>
    /// <returns>
    ///   The <see cref="Column" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static Column<T> From(IColumn column) {
      if (column == null) {
        throw new ArgumentNullException("column");
      }

      if (typeof(T) != column.Type) {
        throw new ArgumentException("The argument does not represent a column of the correct generic type");
      }

      return new Column<T> {
                             Name = column.Name, 
                             DbType = column.DbType, 
                             DbName = column.DbName, 
                             Precision = column.Precision, 
                             Scale = column.Scale, 
                             Length = column.Length, 
                             Ignore = column.Ignore, 
                             ExcludeByDefault = column.ExcludeByDefault, 
                             Relationship = column.Relationship, 
                           };
    }
  }
}