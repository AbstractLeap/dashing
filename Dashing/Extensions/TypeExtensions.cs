namespace Dashing.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq;

    /// <summary>
    ///     The type extensions.
    /// </summary>
    public static class TypeExtensions {
        /// <summary>
        ///     The type map.
        /// </summary>
        private static readonly IDictionary<Type, DbType> TypeMap = new Dictionary<Type, DbType> {
                                                                                                     { typeof(byte), DbType.Byte },
                                                                                                     { typeof(sbyte), DbType.SByte },
                                                                                                     { typeof(short), DbType.Int16 },
                                                                                                     { typeof(ushort), DbType.UInt16 },
                                                                                                     { typeof(int), DbType.Int32 },
                                                                                                     { typeof(uint), DbType.UInt32 },
                                                                                                     { typeof(long), DbType.Int64 },
                                                                                                     { typeof(ulong), DbType.UInt64 },
                                                                                                     { typeof(float), DbType.Single },
                                                                                                     { typeof(double), DbType.Double },
                                                                                                     { typeof(decimal), DbType.Decimal },
                                                                                                     { typeof(bool), DbType.Boolean },
                                                                                                     { typeof(string), DbType.String },
                                                                                                     { typeof(char), DbType.StringFixedLength },
                                                                                                     { typeof(Guid), DbType.Guid },
                                                                                                     { typeof(DateTime), DbType.DateTime },
                                                                                                     { typeof(DateTimeOffset), DbType.DateTimeOffset },
                                                                                                     { typeof(byte[]), DbType.Binary },
                                                                                                     { typeof(byte?), DbType.Byte },
                                                                                                     { typeof(sbyte?), DbType.SByte },
                                                                                                     { typeof(short?), DbType.Int16 },
                                                                                                     { typeof(ushort?), DbType.UInt16 },
                                                                                                     { typeof(int?), DbType.Int32 },
                                                                                                     { typeof(uint?), DbType.UInt32 },
                                                                                                     { typeof(long?), DbType.Int64 },
                                                                                                     { typeof(ulong?), DbType.UInt64 },
                                                                                                     { typeof(float?), DbType.Single },
                                                                                                     { typeof(double?), DbType.Double },
                                                                                                     { typeof(decimal?), DbType.Decimal },
                                                                                                     { typeof(bool?), DbType.Boolean },
                                                                                                     { typeof(char?), DbType.StringFixedLength },
                                                                                                     { typeof(Guid?), DbType.Guid },
                                                                                                     { typeof(DateTime?), DbType.DateTime },
                                                                                                     { typeof(DateTimeOffset?), DbType.DateTimeOffset },
                                                                                                     { typeof(Binary), DbType.Binary },
                                                                                                     { typeof(TimeSpan), DbType.Time },
                                                                                                     { typeof(TimeSpan?), DbType.Time }
                                                                                                 };

        private static readonly IDictionary<DbType, Type> DbTypeMap = new Dictionary<DbType, Type> {
                                                                                                       { DbType.Int32, typeof(int) },
                                                                                                       { DbType.Int16, typeof(short) },
                                                                                                       { DbType.Int64, typeof(long) },
                                                                                                       { DbType.UInt16, typeof(ushort) },
                                                                                                       { DbType.UInt32, typeof(uint) },
                                                                                                       { DbType.UInt64, typeof(ulong) },
                                                                                                       { DbType.Byte, typeof(byte) },
                                                                                                       { DbType.SByte, typeof(sbyte) },
                                                                                                       { DbType.String, typeof(string) },
                                                                                                       { DbType.AnsiString, typeof(string) },
                                                                                                       { DbType.AnsiStringFixedLength, typeof(string) },
                                                                                                       { DbType.StringFixedLength, typeof(string) },
                                                                                                       { DbType.Guid, typeof(Guid) },
                                                                                                       { DbType.Time, typeof(TimeSpan) }
                                                                                                   };

        public static string DefaultFor(this DbType dbType, bool isNullable) {
            switch (dbType) {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return isNullable ? null : string.Empty;
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return isNullable ? null : "0";
                case DbType.Binary:
                    return null;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Time:
                    return isNullable ? null : "current_timestamp";
                case DbType.Guid:
                    return isNullable ? null : "newid()";
                case DbType.DateTimeOffset:
                    return isNullable ? null : "sysdatetimeoffset()";
                case DbType.Object:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(
                        "dbType",
                        "Unknown Db Type for Default value resolution");
            }
        }

        /// <summary>
        ///     The is entity type.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsEntityType(this Type type) {
            return !type.IsValueType && type != typeof(string) && type != typeof(byte[]);
        }

        /// <summary>
        ///     The is collection.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsCollection(this Type type) {
            return type != typeof(byte[]) && type != typeof(string) && type.IsImplementationOf(typeof(IEnumerable));
        }

        /// <summary>
        ///     The is implementation of.
        /// </summary>
        /// <param name="thisType">
        ///     The this type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsImplementationOf(this Type thisType, Type type) {
            return null != thisType.GetInterface(type.FullName);
        }

        /// <summary>
        ///     The get db type.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The <see cref="DbType" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public static DbType GetDbType(this Type type) {
            if (type.IsNullable()) {
                type = type.GetGenericArguments()[0];
            }

            if (TypeMap.ContainsKey(type)) {
                return TypeMap[type];
            }

            // just use underlying type of enum
            if (!type.IsEnum) {
                throw new ArgumentOutOfRangeException("type", "Unable to determine the DBType for type: " + type.FullName);
            }

            var enumType = Enum.GetUnderlyingType(type);
            if (TypeMap.ContainsKey(enumType)) {
                return TypeMap[enumType];
            }

            throw new ArgumentOutOfRangeException("type", "Unable to determine the DBType for type: " + type.FullName);
        }

        /// <summary>
        ///     Provides the CLR type for a particular DbType
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type GetCLRType(this DbType dbType) {
            if (!DbTypeMap.ContainsKey(dbType)) {
                throw new ArgumentOutOfRangeException("dbType", "Unable to determine the type for dbType: " + dbType);
            }

            return DbTypeMap[dbType];
        }

        /// <summary>
        ///     The is nullable.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsNullable(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        ///     Return all the implemented or inherited interfaces and the full hierarchy of base types
        /// </summary>
        /// <param name="type">The type to inspect</param>
        /// <returns>The ancestor types</returns>
        public static IEnumerable<Type> GetAncestorTypes(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces()) {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null) {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}