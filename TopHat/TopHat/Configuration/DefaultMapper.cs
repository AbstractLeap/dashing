namespace TopHat.Configuration {
  using System;
  using System.Data;
  using System.Linq;
  using System.Reflection;

  using global::TopHat.Extensions;

  /// <summary>
  ///   The default mapper.
  /// </summary>
  public class DefaultMapper : IMapper {
    /// <summary>
    ///   The _convention.
    /// </summary>
    private readonly IConvention convention;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DefaultMapper" /> class.
    /// </summary>
    /// <param name="convention">
    ///   The convention.
    /// </param>
    public DefaultMapper(IConvention convention) {
      this.convention = convention;
    }

    /// <summary>
    ///   The map for.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="Map" />.
    /// </returns>
    public Map<T> MapFor<T>() {
      var map = new Map<T>();
      this.Build(typeof(T), map);
      return map;
    }

    /// <summary>
    ///   The build.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="map">
    ///   The map.
    /// </param>
    private void Build(Type entity, IMap map) {
      map.Table = this.convention.TableFor(entity);
      map.Schema = this.convention.SchemaFor(entity);
      map.PrimaryKey = this.convention.PrimaryKeyOf(entity);
      map.Columns = entity.GetProperties().Select(property => this.BuildColumn(entity, property)).ToDictionary(c => c.Name, c => c);
    }

    /// <summary>
    ///   The build column.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="property">
    ///   The property.
    /// </param>
    /// <returns>
    ///   The <see cref="Column" />.
    /// </returns>
    private IColumn BuildColumn(Type entity, PropertyInfo property) {
      var column = (IColumn)Activator.CreateInstance(typeof(Column<>).MakeGenericType(property.PropertyType));
      column.Name = property.Name;
      column.Ignore = !(property.CanRead && property.CanWrite);

      this.ResolveRelationship(entity, property, column);
      this.ApplyAnnotations(entity, property, column);

      return column;
    }

    /// <summary>
    ///   The resolve relationship.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="property">
    ///   The property.
    /// </param>
    /// <param name="column">
    ///   The column.
    /// </param>
    private void ResolveRelationship(Type entity, PropertyInfo property, IColumn column) {
      var propertyName = property.Name;
      var propertyType = property.PropertyType;

      // need to determine the type of the column
      // and then treat accordingly
      if (propertyType.IsEntityType()) {
        if (propertyType.IsCollection()) {
          // assume to be OneToMany
          column.Relationship = RelationshipType.OneToMany;
        }
        else {
          column.Relationship = RelationshipType.ManyToOne;
          column.Name = propertyName + "Id";

          // TODO resolve column type of related primary key - be careful with infinite loops!
        }
      }
      else {
        column.Relationship = RelationshipType.None;
        column.DbType = propertyType.GetDbType();

        // check particular types for defaults
        switch (column.DbType) {
          case DbType.Decimal:
            column.Precision = this.convention.DecimalPrecisionFor(entity, propertyName);
            column.Scale = this.convention.DecimalScaleFor(entity, propertyName);
            break;

          case DbType.String:
            column.Length = this.convention.StringLengthFor(entity, propertyName);
            break;
        }

        // TODO Add nullable column types
      }
    }

    /// <summary>
    ///   The apply annotations.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="property">
    ///   The property.
    /// </param>
    /// <param name="column">
    ///   The column.
    /// </param>
    private void ApplyAnnotations(Type entity, PropertyInfo property, IColumn column) {
      /* should do something, innit! */
    }
  }
}