namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    ///     The map extensions.
    /// </summary>
    public static class MapExtensions {
        /// <summary>
        ///     The table.
        /// </summary>
        /// <param name="map">
        ///     The map.
        /// </param>
        /// <param name="tableName">
        ///     The table name.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
        public static IMap<T> Table<T>(this IMap<T> map, string tableName) {
            map.Table = tableName;
            return map;
        }

        /// <summary>
        ///     The schema.
        /// </summary>
        /// <param name="map">
        ///     The map.
        /// </param>
        /// <param name="schema">
        ///     The schema.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
        public static IMap<T> Schema<T>(this IMap<T> map, string schema) {
            map.Schema = schema;
            return map;
        }

        /// <summary>
        ///     The primary key.
        /// </summary>
        /// <param name="map">
        ///     The map.
        /// </param>
        /// <param name="expression">
        ///     The expression.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
        public static IMap<T> PrimaryKey<T, TResult>(this IMap<T> map, Expression<Func<T, TResult>> expression) {
            foreach (var column in map.Columns.Values) {
                column.IsPrimaryKey = false;
            }

            map.PrimaryKey = map.Columns[NameFromMemberExpression(expression)];
            map.PrimaryKey.IsPrimaryKey = true;
            return map;
        }

        /////// <summary>
        ///////   The index.
        /////// </summary>
        /////// <param name="map">
        ///////   The map.
        /////// </param>
        /////// <param name="newExpression">
        ///////   The new expression.
        /////// </param>
        /////// <typeparam name="T">
        /////// </typeparam>
        /////// <typeparam name="TProperty">
        /////// </typeparam>
        /////// <returns>
        ///////   The <see cref="Map" />.
        /////// </returns>
        /////// <exception cref="NotImplementedException">
        /////// </exception>
        ////public static IMap<T> Index<T, TProperty>(this IMap<T> map, Expression<Func<T, TProperty>> newExpression) {
        ////  throw new NotImplementedException();
        ////}

        /// <summary>
        ///     The property.
        /// </summary>
        /// <param name="map">
        ///     The map.
        /// </param>
        /// <param name="expression">
        ///     The expression.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Column" />.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// </exception>
        public static Column<TProperty> Property<T, TProperty>(this IMap<T> map, Expression<Func<T, TProperty>> expression) {
            var columnName = NameFromMemberExpression(expression);

            IColumn column;
            if (!map.Columns.TryGetValue(columnName, out column)) {
                throw new KeyNotFoundException();
            }

            var columnT = column as Column<TProperty>;

            if (columnT == null) {
                map.Columns[columnName] = columnT = Column<TProperty>.From(column); // lift the Column into a Column<T>
            }

            return columnT;
        }

        /// <summary>
        ///     The name from member expression.
        /// </summary>
        /// <param name="expression">
        ///     The expression.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static string NameFromMemberExpression<T, TResult>(Expression<Func<T, TResult>> expression) {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) {
                throw new ArgumentException("expression must be a MemberExpression");
            }

            return memberExpression.Member.Name;
        }
    }
}