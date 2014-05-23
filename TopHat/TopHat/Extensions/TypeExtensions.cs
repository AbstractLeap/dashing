namespace TopHat.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq;

    /// <summary>
    ///   The type extensions.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        ///   The type map.
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
                                                                                               { typeof(Binary), DbType.Binary }
                                                                                             };

        private static readonly IDictionary<DbType, Type> DbTypeMap = new Dictionary<DbType, Type> {
          { DbType.Int32, typeof(int) },
          {DbType.Int16, typeof(short)},
          {DbType.Int64, typeof(long)},
          {DbType.UInt16, typeof(ushort)},
          {DbType.UInt32, typeof(uint)},
          {DbType.UInt64, typeof(ulong)},
          {DbType.Byte, typeof(byte)},
          {DbType.SByte, typeof(sbyte)},
          {DbType.String, typeof(string)},
          {DbType.Guid, typeof(Guid)}
          };

        /// <summary>
        ///   The is entity type.
        /// </summary>
        /// <param name="type">
        ///   The type.
        /// </param>
        /// <returns>
        ///   The <see cref="bool" />.
        /// </returns>
        public static bool IsEntityType(this Type type)
        {
            return !type.IsValueType && type != typeof(String) && type != typeof(byte[]);
        }

        /// <summary>
        ///   The is collection.
        /// </summary>
        /// <param name="type">
        ///   The type.
        /// </param>
        /// <returns>
        ///   The <see cref="bool" />.
        /// </returns>
        public static bool IsCollection(this Type type)
        {
            return type != typeof(byte[]) && type != typeof(string) && type.IsImplementationOf(typeof(IEnumerable));
        }

        /// <summary>
        ///   The is implementation of.
        /// </summary>
        /// <param name="thisType">
        ///   The this type.
        /// </param>
        /// <param name="type">
        ///   The type.
        /// </param>
        /// <returns>
        ///   The <see cref="bool" />.
        /// </returns>
        public static bool IsImplementationOf(this Type thisType, Type type)
        {
            return null != thisType.GetInterface(type.FullName);
        }

        /// <summary>
        ///   The get db type.
        /// </summary>
        /// <param name="type">
        ///   The type.
        /// </param>
        /// <returns>
        ///   The <see cref="DbType" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public static DbType GetDbType(this Type type)
        {
            if (type.IsNullable())
            {
                type = type.GetGenericArguments()[0];
            }

            if (TypeMap.ContainsKey(type))
            {
                return TypeMap[type];
            }

            // just use underlying type of enum
            if (!type.IsEnum)
            {
                throw new ArgumentOutOfRangeException("type", "Unable to determine the DBType for type: " + type.FullName);
            }

            var enumType = Enum.GetUnderlyingType(type);
            if (TypeMap.ContainsKey(enumType))
            {
                return TypeMap[enumType];
            }

            throw new ArgumentOutOfRangeException("type", "Unable to determine the DBType for type: " + type.FullName);
        }

        /// <summary>
        /// Provides the CLR type for a particular DbType
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type GetCLRType(this DbType dbType)
        {
            if (!DbTypeMap.ContainsKey(dbType))
            {
                throw new ArgumentOutOfRangeException("dbType", "Unable to determine the type for dbType: " + dbType.ToString());
            }

            return DbTypeMap[dbType];
        }

        /// <summary>
        ///   The is nullable.
        /// </summary>
        /// <param name="type">
        ///   The type.
        /// </param>
        /// <returns>
        ///   The <see cref="bool" />.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}