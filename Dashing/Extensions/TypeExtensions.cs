namespace Dashing.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using Dashing.Versioning;
#if !COREFX
    using System.Data.Linq;
#endif



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
                                                                                                     { typeof(DateTime), DbType.DateTime2 },
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
                                                                                                     { typeof(DateTime?), DbType.DateTime2 },
                                                                                                     { typeof(DateTimeOffset?), DbType.DateTimeOffset },
#if !COREFX
            { typeof(Binary), DbType.Binary },
#endif
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
                                                                                                       { DbType.Time, typeof(TimeSpan) },
                                                                                                       { DbType.Boolean, typeof(bool)},
                                                                                                       { DbType.Date, typeof(DateTime) },
                                                                                                       { DbType.DateTime, typeof(DateTime) },
                                                                                                       { DbType.DateTime2, typeof(DateTime) },
                                                                                                       { DbType.DateTimeOffset, typeof(DateTimeOffset) },
                                                                                                       { DbType.Binary, typeof(byte[]) },
                                                                                                       { DbType.Single, typeof(float) },
                                                                                                       { DbType.Double, typeof(double) },
                                                                                                       { DbType.Decimal, typeof(decimal) }
                                                                                                   };
        
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
            return !type.IsValueType() && type != typeof(string) && type != typeof(byte[]);
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
            return type.IsAssignableFrom(thisType);
        }

        /// <summary>
        /// Indicates that the type implements IVersionedEntity
        /// </summary>
        /// <param name="thisType"></param>
        /// <returns></returns>
        public static bool IsVersionedEntity(this Type thisType) {
            return thisType.GetInterfaces().Any(i => i.IsGenericType() && i.GetGenericTypeDefinition() == typeof(IVersionedEntity<>));
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
            if (!type.IsEnum()) {
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
            return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
            var currentBaseType = type.BaseType();
            while (currentBaseType != null) {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType();
            }
        }

        public static bool TypeTakesLength(this DbType type) {
            switch (type) {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Binary:
                case DbType.String:
                case DbType.StringFixedLength:
                    return true;

                default:
                    return false;
            }
        }

        public static bool TypeTakesPrecisionAndScale(this DbType type) {
            switch (type) {
                case DbType.Decimal:
                    return true;

                default:
                    return false;
            }
        }

        public static Type GetEnumerableType(this Type type) {
            if (type.IsArray) {
                return type.GetElementType();
            }

            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                return type.GetGenericArguments()[0];
            }

            var parameterTypeCandidates = type.GetInterfaces()
                                              .Where(t => t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                              .Select(t => t.GetGenericArguments()[0])
                                              .ToArray();
            if (parameterTypeCandidates.Length == 1) {
                return parameterTypeCandidates[0];
            }

            return null;
        }

        public static bool IsValueType(this Type type) {
#if COREFX
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static bool IsEnum(this Type type) {
#if COREFX
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type) {
#if COREFX
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type BaseType(this Type type) {
#if COREFX
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static Assembly Assembly(this Type type) {
#if COREFX
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static bool IsClass(this Type type) {
#if COREFX
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsPublic(this Type type)
        {
#if COREFX
            return type.GetTypeInfo().IsPublic;
#else
            return type.IsPublic;
#endif
        }

        public static bool IsAbstract(this Type type) {
#if COREFX
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }
    }
}