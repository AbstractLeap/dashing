namespace TopHat.Tests.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using TopHat.Configuration;

    public static class ObjectPopulationExtensions {
        public static T Populate<T>(this T obj, params object[] defaults) {
            var defaultsDict = defaults.ToDictionary(o => o.GetType(), o => o);
            foreach (var d in defaults) {
                foreach (var i in d.GetType().GetInterfaces()) {
                    defaultsDict[i] = d;
                }
            }

            foreach (var prop in obj.GetType().GetProperties()) {
                if (defaultsDict.ContainsKey(prop.PropertyType)) {
                    prop.SetValue(obj, defaultsDict[prop.PropertyType]);
                }
                else if (prop.PropertyType == typeof(string)) {
                    prop.SetValue(obj, "foo");
                }
                else if (prop.PropertyType == typeof(bool)) {
                    prop.SetValue(obj, true);
                }
                else if (prop.PropertyType == typeof(int)) {
                    prop.SetValue(obj, 7);
                }
                else if (prop.PropertyType == typeof(ushort)) {
                    prop.SetValue(obj, (ushort)7);
                }
                else if (prop.PropertyType == typeof(byte)) {
                    prop.SetValue(obj, (byte)7);
                }
                else if (prop.PropertyType == typeof(Type)) {
                    prop.SetValue(obj, typeof(string));
                }
                else if (prop.PropertyType.IsEnum) {
                    if (prop.PropertyType.IsEnumDefined(1)) {
                        prop.SetValue(obj, 1);
                    }
                }
                else if (prop.PropertyType.IsGenericType) {
                    if (obj.GetType().GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                        var i = prop.PropertyType.GetInterface(typeof(IDictionary).FullName);
                        var t = typeof(Dictionary<,>).MakeGenericType(i.GenericTypeArguments[0], i.GenericTypeArguments[1]);
                        prop.SetValue(obj, Activator.CreateInstance(t));
                    }
                }
                else if (prop.PropertyType == typeof(IMap)) {
                    prop.SetValue(obj, new Map<string>());
                }
                else {
                    throw new Exception(string.Format("Unexpected type {0} when trying to populate {1}.{2}", prop.PropertyType.Name, typeof(T).Name, prop.Name));
                }
            }

            return obj;
        }

        private static bool IsImplementationOf(this Type thisType, Type type) {
            return null != thisType.GetInterface(type.FullName);
        }
    }
}