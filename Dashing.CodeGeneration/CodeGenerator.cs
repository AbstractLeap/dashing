using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dashing.CodeGeneration
{
    public class CodeGenerator
    {
        [Fact]
        public void Main()
        {
            var sb = new StringBuilder();
            // SqlQuerySelection
            for (var i = 1; i <= 16; i++)
            {
                var types = GetTypes(i);
                sb.AppendLine($"public class SqlQuerySelection<{types}, TResult> : IEnumerable<TResult> {{");
                sb.AppendLine($"private readonly Expression<Func<{types}, TResult>> selectExpression;");
                sb.AppendLine($"public SqlQuerySelection(Expression<Func<{types}, TResult>> selectExpression) {{");
                sb.AppendLine("this.selectExpression = selectExpression;");
                sb.AppendLine("}");
                sb.Append(@"
public IEnumerator<TResult> GetEnumerator()
{
    throw new NotImplementedException();
}

IEnumerator IEnumerable.GetEnumerator()
{
    throw new NotImplementedException();
}

public Task<IEnumerable<TResult>> EnumerateAsync() {
    throw new NotImplementedException();
}
");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            // From Builder
            for (var i = 1; i <= 16; i++)
            {
                var types = GetTypes(i);
                sb.Append($@"public class SqlFromDefinition<{types}> {{

    private readonly IList<Expression<Func<{types}, bool>>> whereExpressions = new List<Expression<Func<{types}, bool>>>();

    private readonly IList<Expression<Func<{types}, bool>>> havingExpressions = new List<Expression<Func<{types}, bool>>>();

    private readonly IList<Expression> groupByExpressions = new List<Expression>();

    private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();

                ");
                if (i > 1)
                {
                    sb.Append($@"
    private readonly JoinType joinType;

    private readonly Expression<Func<{types}, bool>> joinExpression;

public SqlFromDefinition(JoinType joinType) {{
                        this.joinType = joinType;
                        }}

public SqlFromDefinition(JoinType joinType, Expression<Func<{types}, bool>> joinExpression) {{
        this.joinType = joinType;
        this.joinExpression = joinExpression;
    }}
");
                }

                if (i < 16)
                {
                    var nextTypes = GetTypes(i + 1);
                    var joinTypes = new[] { "InnerJoin", "LeftJoin", "RightJoin", "FullOuterJoin" };
                    foreach (var joinType in joinTypes)
                    {
                        sb.AppendLine($"public SqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>() {{");
                        sb.AppendLine($"return new SqlFromDefinition<{nextTypes}>(JoinType.{joinType});");
                        sb.AppendLine("}");

                        sb.AppendLine($"public SqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>(Expression<Func<{nextTypes}, bool>> joinExpression) {{");
                        sb.AppendLine($"return new SqlFromDefinition<{nextTypes}>(JoinType.{joinType}, joinExpression);");
                        sb.AppendLine("}");
                    }
                }

                sb.AppendLine($"public SqlFromDefinition<{types}> Where(Expression<Func<{types}, bool>> whereExpression) {{");
                sb.AppendLine($"this.whereExpressions.Add(whereExpression);");
                sb.AppendLine("return this;");
                sb.AppendLine("}");

                sb.AppendLine($"public SqlFromDefinition<{types}> Having(Expression<Func<{types}, bool>> havingExpression) {{");
                sb.AppendLine($"this.havingExpressions.Add(havingExpression);");
                sb.AppendLine("return this;");
                sb.AppendLine("}");

                sb.AppendLine($"public SqlFromDefinition<{types}> GroupBy<TResult>(Expression<Func<{types}, TResult>> groupByExpression) {{");
                sb.AppendLine($"this.groupByExpressions.Add(groupByExpression);");
                sb.AppendLine("return this;");
                sb.AppendLine("}");

                sb.AppendLine($"public SqlFromDefinition<{types}> OrderBy<TResult>(Expression<Func<{types}, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending) {{");
                sb.AppendLine($"this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));");
                sb.AppendLine("return this;");
                sb.AppendLine("}");

                sb.Append($@"

   public IEnumerable<TResult> Select<TResult>(Expression<Func<{types}, TResult>> selectExpression)
        {{
                        return new SqlQuerySelection<{types}, TResult>(selectExpression);
                    }}");

                sb.AppendLine("}");
                sb.AppendLine();
            }

            var code = sb.ToString();
            Console.Write(code);
        }

        private static string GetTypes(int i)
        {
            return string.Join(", ", Enumerable.Range(1, i).Select(idx => idx == 1 ? "T" : $"T{idx}"));
        }
    }
}
