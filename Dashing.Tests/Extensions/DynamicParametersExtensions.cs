namespace Dashing.Tests.Extensions {
    using System;
    using System.Reflection;

    using Dapper;

    public static class DynamicParametersExtensions {
        // TODO: Put this in an extension method
        public static object GetValueOfParameter(this DynamicParameters p, string parameterName) {
            var parametersField = typeof(DynamicParameters).GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance);
            if (parametersField == null) {
                throw new Exception("Could not reflect the parameters field of the DynamicParameters object");
            }

            dynamic parameters = parametersField.GetValue(p);

            foreach (var paramInfoPair in parameters) {
                var paramInfo = paramInfoPair.GetType().GetProperty("Value").GetValue(paramInfoPair);
                var paramName = paramInfo.GetType().GetProperty("Name").GetValue(paramInfo);
                var paramValue = paramInfo.GetType().GetProperty("Value").GetValue(paramInfo);

                if (paramName == parameterName) {
                    return paramValue;
                }
            }

            return null;
        }
    }
}