namespace Dashing.Testing.Tests {
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExpressionExtensions {
        public static string ToDebugString(this Expression expression) {
            if (expression == null)
                return null;

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(expression) as string;
        }
    }
}