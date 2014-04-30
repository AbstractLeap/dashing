using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Extensions
{
    internal static class TypeExtensions
    {
        internal static IDictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>
        {
            {typeof(byte) , DbType.Byte},
            {typeof(sbyte) , DbType.SByte},
            {typeof(short) , DbType.Int16},
            {typeof(ushort) , DbType.UInt16},
            {typeof(int) , DbType.Int32},
            {typeof(uint) , DbType.UInt32},
            {typeof(long) , DbType.Int64},
            {typeof(ulong) , DbType.UInt64},
            {typeof(float) , DbType.Single},
            {typeof(double) , DbType.Double},
            {typeof(decimal) , DbType.Decimal},
            {typeof(bool) , DbType.Boolean},
            {typeof(string) , DbType.String},
            {typeof(char) , DbType.StringFixedLength},
            {typeof(Guid) , DbType.Guid},
            {typeof(DateTime) , DbType.DateTime},
            {typeof(DateTimeOffset) , DbType.DateTimeOffset},
            {typeof(byte[]) , DbType.Binary},
            {typeof(byte?) , DbType.Byte},
            {typeof(sbyte?) , DbType.SByte},
            {typeof(short?) , DbType.Int16},
            {typeof(ushort?) , DbType.UInt16},
            {typeof(int?) , DbType.Int32},
            {typeof(uint?) , DbType.UInt32},
            {typeof(long?) , DbType.Int64},
            {typeof(ulong?) , DbType.UInt64},
            {typeof(float?) , DbType.Single},
            {typeof(double?) , DbType.Double},
            {typeof(decimal?) , DbType.Decimal},
            {typeof(bool?) , DbType.Boolean},
            {typeof(char?) , DbType.StringFixedLength},
            {typeof(Guid?) , DbType.Guid},
            {typeof(DateTime?) , DbType.DateTime},
            {typeof(DateTimeOffset?) , DbType.DateTimeOffset},
            {typeof(System.Data.Linq.Binary) , DbType.Binary}
        };

        public static bool IsCollection(this Type type)
        {
            return type != typeof(byte[])
                && type != typeof(string)
                && type.IsImplementationOf(typeof(IEnumerable));
        }

        public static bool IsImplementationOf(this Type thisType, Type type)
        {
            return null != thisType.GetInterface(type.FullName);
        }

        public static DbType GetDBType(this Type type)
        {
            if (type.IsNullable())
            {
                type = type.GetGenericArguments()[0];
            }

            if (typeMap.ContainsKey(type))
            {
                return typeMap[type];
            }

            // just use underlying type of enum
            if (type.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(type);
                if (typeMap.ContainsKey(enumType))
                {
                    return typeMap[enumType];
                }
            }

            throw new ArgumentOutOfRangeException("type", "Unable to determine the DBType for type: " + type.FullName);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}