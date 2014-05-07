using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TopHat.Configuration {
	public static class MapExtensions {
		public static Map<T> Table<T>(this Map<T> map, string tableName) {
			map.Table = tableName;
			return map;
		}

		public static Map<T> Schema<T>(this Map<T> map, string schema) {
			map.Schema = schema;
			return map;
		}

		public static Map<T> PrimaryKey<T, TResult>(this Map<T> map, Expression<Func<T, TResult>> expression) {
			map.PrimaryKey = NameFromMemberExpression(expression);
			return map;
		}

		public static Map<T> PrimaryKeyIsGenerated<T>(this Map<T> map) {
			map.IsPrimaryKeyDatabaseGenerated = true;
			return map;
		}

		public static Map<T> PrimaryKeyIsNotGenerated<T>(this Map<T> map) {
			map.IsPrimaryKeyDatabaseGenerated = false;
			return map;
		}

		public static Map<T> Index<T, TProperty>(this Map<T> map, Expression<Func<T, TProperty>> newExpression) {
			throw new NotImplementedException();
		}

		public static Column<TProperty> Property<T, TProperty>(this Map<T> map, Expression<Func<T, TProperty>> expression) {
			var columnName = NameFromMemberExpression(expression);

			Column column;
			if (!map.Columns.TryGetValue(columnName, out column))
				throw new KeyNotFoundException();

			var columnT = column as Column<TProperty>;

			if (columnT == null)
				map.Columns[columnName] = columnT = Column<TProperty>.From(column); // lift the Column into a Column<T>

			return columnT;
		}

		private static string NameFromMemberExpression<T, TResult>(Expression<Func<T, TResult>> expression) {
			var memberExpression = expression.Body as MemberExpression;
			if (memberExpression == null) throw new ArgumentException("expression must be a MemberExpression");
			return memberExpression.Member.Name;
		}
	}
}