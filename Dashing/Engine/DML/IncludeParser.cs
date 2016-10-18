namespace Dashing.Engine.DML {
    using System;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    public class IncludeExcludeParser {
        private readonly IConfiguration configuration;

        public IncludeExcludeParser(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public IColumn ParseExpression(Expression expression) {
            var lambda = expression as LambdaExpression;
            if (lambda == null) {
                throw new InvalidOperationException("Include and Exclude expressions must be LambdaExpressions");
            }

            var memberExpression = lambda.Body as MemberExpression;
            if (memberExpression == null) {
                throw new InvalidOperationException("Include and Exclude expressions must use a MemberExpression to access the property");
            }

            var domainType = memberExpression.Expression.Type;
            var map = this.configuration.GetMap(domainType);
            return map.Columns[memberExpression.Member.Name];
        }
    }
}