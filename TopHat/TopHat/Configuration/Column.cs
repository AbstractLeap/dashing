using System;
using System.Data;

namespace TopHat.Configuration {
	public class Column {
		public Column(Type type) {
			if (type == null) throw new ArgumentNullException("type");

			Type = type;
		}

		public Type Type { get; private set; }

		public string Name { get; set; }

		public DbType DbType { get; set; }

		public byte Precision { get; set; }

		public byte Scale { get; set; }

		public ushort Length { get; set; }

		/// <summary>
		///   Indicates whether the column will be ignored for all queries and schema generation
		/// </summary>
		public bool Ignore { get; set; }

		/// <summary>
		///   Indicates whether the column will be excluded from select queries unless specifically requested
		/// </summary>
		public bool ExcludedByDefault { get; set; }

		public RelationshipType Relationship { get; set; }

		/*
		 * mark doing optimization
		public string ColumnTypeString { get; set; }
		 */
	}

	public class Column<T> : Column {
		public Column()
			: base(typeof (T)) {}

		public static Column<T> From(Column column) {
			if (typeof (T) != column.Type) throw new ArgumentException("The argument does not represent a column of the correct generic type");

			return new Column<T> {
				Name = column.Name,
				DbType = column.DbType,
				Precision = column.Precision,
				Scale = column.Scale,
				Length = column.Length,
				Ignore = column.Ignore,
				ExcludedByDefault = column.ExcludedByDefault,
				Relationship = column.Relationship,
				/*
				ColumnTypeString = column.ColumnTypeString
				 */
			};
		}
	}
}