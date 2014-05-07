using System.Data;

/*
		/// <summary>
		///   Indicates whether the column will be excluded from select queries unless specifically requested
		/// </summary>
		public bool ExcludedByDefault { get; set; }

		public RelationshipType Relationship { get; set; }
*/

namespace TopHat.Configuration {
	public static class ColumnExtensions {
		public static Column<T> Name<T>(this Column<T> column, string name) {
			column.Name = name;
			return column;
		}

		public static Column<T> DbType<T>(this Column<T> column, DbType dbType) {
			column.DbType = dbType;
			return column;
		}

		public static Column<T> Precision<T>(this Column<T> column, byte precision) {
			column.Precision = precision;
			return column;
		}

		public static Column<T> Scale<T>(this Column<T> column, byte scale) {
			column.Scale = scale;
			return column;
		}

		public static Column<T> Length<T>(this Column<T> column, ushort length) {
			column.Length = length;
			return column;
		}

		public static Column<T> ExcludeByDefault<T>(this Column<T> column) {
			column.ExcludedByDefault = true;
			return column;
		}

		public static Column<T> DontExcludeByDefault<T>(this Column<T> column) {
			column.ExcludedByDefault = false;
			return column;
		}

		public static Column<T> Ignore<T>(this Column<T> column) {
			column.Ignore = true;
			return column;
		}

		public static Column<T> DontIgnore<T>(this Column<T> column) {
			column.Ignore = false;
			return column;
		}
	}
}