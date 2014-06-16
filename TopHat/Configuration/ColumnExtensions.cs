namespace TopHat.Configuration {
    using System;
    using System.Data;

    /// <summary>
    ///     The column extensions.
    /// </summary>
    public static class ColumnExtensions {
        /// <summary>
        ///     The name.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn Name(this IColumn column, string name) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.Name = name;
            return column;
        }

        /// <summary>
        ///     The db type.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <param name="dbType">
        ///     The db type.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn DbType(this IColumn column, DbType dbType) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.DbType = dbType;
            return column;
        }

        /// <summary>
        ///     The precision.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <param name="precision">
        ///     The precision.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn Precision(this IColumn column, byte precision) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.Precision = precision;
            return column;
        }

        /// <summary>
        ///     The scale.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <param name="scale">
        ///     The scale.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn Scale(this IColumn column, byte scale) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.Scale = scale;
            return column;
        }

        /// <summary>
        ///     The length.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <param name="length">
        ///     The length.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn Length(this IColumn column, ushort length) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.Length = length;
            return column;
        }

        /// <summary>
        ///     The exclude by default.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn ExcludeByDefault(this IColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.IsExcludedByDefault = true;
            return column;
        }

        /// <summary>
        ///     The dont exclude by default.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn DontExcludeByDefault(this IColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.IsExcludedByDefault = false;
            return column;
        }

        /// <summary>
        ///     The ignore.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn Ignore(this IColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.IsIgnored = true;
            return column;
        }

        /// <summary>
        ///     The dont ignore.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IColumn" />.
        /// </returns>
        public static IColumn DontIgnore(this IColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            column.IsIgnored = false;
            return column;
        }
    }
}