namespace TopHat.Extensions {
    using System;
    using System.Reflection;

    using Dapper;

    public static class DapperExtensions {
        public static object GetValue(this DynamicParameters parameters, string key) {
            var parametersProperty = typeof(DynamicParameters).GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance);
            var explicitParameters = parametersProperty.GetValue(parameters);

            if (explicitParameters != null) {
                foreach (var explicitParameter in (dynamic)explicitParameters) {
                    string paramKey = explicitParameter.GetType().GetProperty("Key").GetValue(explicitParameter, null);
                    var valueProperty = explicitParameter.GetType().GetProperty("Value").GetValue(explicitParameter, null);
                    if (paramKey == key) {
                        return valueProperty.GetType().GetProperty("Value").GetValue(valueProperty, null);
                    }
                }
            }

            throw new ArgumentException("No such key in the parameters dictionary");
        }
    }
}