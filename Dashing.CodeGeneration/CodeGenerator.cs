namespace Dashing.CodeGeneration {
    using System;
    using System.Linq;
    using System.Text;

    using Xunit;
    using Xunit.Abstractions;

    public class SqlBuilder {
        private readonly ITestOutputHelper output;

        public SqlBuilder(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void Main() {
            var sqlQuerySelectionInterfaceBuilder = new StringBuilder();
            var sqlQuerySelectionClassBuilder = new StringBuilder();
            var sqlFromDefinitionInterfaceBuilder = new StringBuilder();
            var sqlFromDefinitionClassBuilder = new StringBuilder();



            // SqlQuerySelection
            for (var i = 1; i <= 16; i++) {
                var types = GetTypes(i);

                // interface
                sqlQuerySelectionInterfaceBuilder.Append(
                    $@"
public interface ISqlQuerySelection<{types}, TResult> : IEnumerable<TResult> {{
    Task<IEnumerable<TResult>> EnumerateAsync();
}}
");

                // class
                sqlQuerySelectionClassBuilder.Append(
                    $@"
public class SqlQuerySelection<{types}, TResult> : ISqlQuerySelection<{types}, TResult> {{
    private readonly Expression<Func<{types}, TResult>> selectExpression;
    
    public SqlQuerySelection(Expression<Func<{types}, TResult>> selectExpression) {{
        this.selectExpression = selectExpression;
    }}

    public IEnumerator<TResult> GetEnumerator()
    {{
        throw new NotImplementedException();
    }}

    IEnumerator IEnumerable.GetEnumerator()
    {{
        throw new NotImplementedException();
    }}

    public Task<IEnumerable<TResult>> EnumerateAsync() {{
        throw new NotImplementedException();
    }}
}}
");
            }

            // From Builder
            for (var i = 1; i <= 16; i++) {
                var types = GetTypes(i);

                // interface
                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
public interface ISqlFromDefinition<{types}> {{
");

                // class
                sqlFromDefinitionClassBuilder.Append(
                    $@"
public class SqlFromDefinition<{types}> : ISqlFromDefinition<{types}> {{

    private readonly IList<Expression<Func<{types}, bool>>> whereExpressions = new List<Expression<Func<{types}, bool>>>();

    private readonly IList<Expression<Func<{types}, bool>>> havingExpressions = new List<Expression<Func<{types}, bool>>>();

    private readonly IList<Expression> groupByExpressions = new List<Expression>();

    private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();

                ");

                if (i > 1) {
                    // join expressions only apply when you have more than 1 table
                    sqlFromDefinitionClassBuilder.Append(
                        $@"
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

                if (i < 16) {
                    var nextTypes = GetTypes(i + 1);
                    var joinTypes = new[] { "InnerJoin", "LeftJoin", "RightJoin", "FullOuterJoin" };
                    foreach (var joinType in joinTypes) {
                        sqlFromDefinitionInterfaceBuilder.Append(
                            $@"
    ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>();
");


                        sqlFromDefinitionClassBuilder.Append(
                            $@"
public ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>() {{
    return new SqlFromDefinition<{nextTypes}>(JoinType.{joinType});
}}
");

                        sqlFromDefinitionInterfaceBuilder.Append(
                            $@"
    ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>(Expression<Func<{nextTypes}, bool>> joinExpression);
");

                        sqlFromDefinitionClassBuilder.Append(
                            $@"
public ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>(Expression<Func<{nextTypes}, bool>> joinExpression) {{
    return new SqlFromDefinition<{nextTypes}>(JoinType.{joinType}, joinExpression);
}}
");
                    }
                }

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlFromDefinition<{types}> Where(Expression<Func<{types}, bool>> whereExpression);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlFromDefinition<{types}> Where(Expression<Func<{types}, bool>> whereExpression) {{
    this.whereExpressions.Add(whereExpression);
return this;
                }}");

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlFromDefinition<{types}> Having(Expression<Func<{types}, bool>> havingExpression);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlFromDefinition<{types}> Having(Expression<Func<{types}, bool>> havingExpression) {{
    this.havingExpressions.Add(havingExpression);
    return this;
}}
");

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlFromDefinition<{types}> GroupBy<TResult>(Expression<Func<{types}, TResult>> groupByExpression);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlFromDefinition<{types}> GroupBy<TResult>(Expression<Func<{types}, TResult>> groupByExpression) {{
    this.groupByExpressions.Add(groupByExpression);
    return this;
}}
");

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlFromDefinition<{types}> OrderBy<TResult>(Expression<Func<{types}, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlFromDefinition<{types}> OrderBy<TResult>(Expression<Func<{types}, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending) {{
    this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
    return this;
}}
");

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlQuerySelection<{types}, TResult> Select<TResult>(Expression<Func<{types}, TResult>> selectExpression);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlQuerySelection<{types}, TResult> Select<TResult>(Expression<Func<{types}, TResult>> selectExpression) {{
    return new SqlQuerySelection<{types}, TResult>(selectExpression);
}}
");
                sqlFromDefinitionInterfaceBuilder.AppendLine("}");
                sqlFromDefinitionClassBuilder.AppendLine("}");
            }

            var sqlQuerySelectionInterfaces = sqlQuerySelectionInterfaceBuilder.ToString();
            var sqlQuerySelectionClasses = sqlQuerySelectionClassBuilder.ToString();
            var sqlFromDefinitionInterfaces = sqlFromDefinitionInterfaceBuilder.ToString();
            var sqlFromDefinitionClasses = sqlFromDefinitionClassBuilder.ToString();
            var code = $@"
{sqlQuerySelectionInterfaces}
{sqlQuerySelectionClasses}
{sqlFromDefinitionInterfaces}
{sqlFromDefinitionClasses}
";
            this.output.WriteLine(code);
        }

        private static string GetTypes(int i) {
            return string.Join(
                ", ",
                Enumerable.Range(1, i)
                          .Select(
                              idx => idx == 1
                                         ? "T"
                                         : $"T{idx}"));
        }
    }
}