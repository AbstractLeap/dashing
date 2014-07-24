namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    public static class ConfigurationHelper {
        public static IMap GetMap(Type type, IDictionary<Type, IMap> mappedTypes, CodeGeneratorConfig config) {
            IMap map;

            // shortcut for simplest case
            if (mappedTypes.TryGetValue(type, out map)) {
                return map;
            }

            // if the type is a generated type
            if (type.Namespace == config.Namespace) {
                if (type.BaseType == null) {
                    throw new ArgumentException("That type is generated but does not have a BaseType");
                }

                return GetMap(type.BaseType, mappedTypes, config);
            }

            // definitely not mapped
            throw new ArgumentException("That type is not mapped");
        }

        public static bool HasMap(Type type, IDictionary<Type, IMap> mappedTypes, CodeGeneratorConfig config) {
            if (mappedTypes.ContainsKey(type)) {
                return true;
            }

            // if the type is a generated type
            if (type.Namespace == config.Namespace) {
                return type.BaseType != null && HasMap(type.BaseType, mappedTypes, config);
            }

            return false;
        }
    }
}