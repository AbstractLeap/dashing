using System;
using System.Data;
using System.Linq;
using System.Reflection;
using TopHat.Extensions;

namespace TopHat.Configuration {
	public class DefaultMapper : IMapper {
		private readonly IConvention _convention;

		public DefaultMapper(IConvention convention) {
			_convention = convention;
		}

		public Map MapFor(Type entity) {
			var map = new Map(entity);
			Build(entity, map);
			return map;
		}

		public Map<T> MapFor<T>() {
			var map = new Map<T>();
			Build(typeof (T), map);
			return map;
		}

		private void Build(Type entity, Map map) {
			map.Table = _convention.TableFor(entity);
			map.Schema = _convention.SchemaFor(entity);
			map.PrimaryKey = _convention.PrimaryKeyOf(entity);
			map.Columns = entity.GetProperties()
			                    .Select(property => BuildColumn(entity, property))
			                    .ToDictionary(c => c.Name, c => c);
		}

		private Column BuildColumn(Type entity, PropertyInfo property) {
			var column = new Column(property.PropertyType) {
				Name = property.Name,
				Ignore = !(property.CanRead && property.CanWrite)
			};

			ResolveRelationship(entity, property, column);
			ApplyAnnotations(entity, property, column);

			return column;
		}

		private void ResolveRelationship(Type entity, PropertyInfo property, Column column) {
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
						column.Precision = _convention.DecimalPrecisionFor(entity, propertyName);
						column.Scale = _convention.DecimalScaleFor(entity, propertyName);
						break;

					case DbType.String:
						column.Length = _convention.StringLengthFor(entity, propertyName);
						break;
				}

				// TODO Add nullable column types
			}
		}

		private void ApplyAnnotations(Type entity, PropertyInfo property, Column column) {
			/* should do something, innit! */
		}
	}
}