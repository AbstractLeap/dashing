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
            var sqlBuilderBuilderBuilder = new StringBuilder(); // that's right, 3 builders in the name!


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
    public Expression<Func<{types}, TResult>> SelectExpression {{ get; set; }}
    
    public SqlFromDefinition<{types}> FromDefinition {{ get; set; }}

    public SqlQuerySelection(SqlFromDefinition<{types}> fromDefinition, Expression<Func<{types}, TResult>> selectExpression) {{
        this.SelectExpression = selectExpression;
        this.FromDefinition = fromDefinition;
    }}

    public CommandDefinition Build() {{
        var builder = new SqlBuilderBuilder<{types}, TResult>(this.FromDefinition, this.SelectExpression, this.FromDefinition{(i == 1 ? string.Empty : ".")}{string.Join(".", Enumerable.Range(1, i - 1).Select(_ => "PreviousFromDefinition"))}.Session);
        return builder.Build();
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

    public IList<Expression<Func<{types}, bool>>> WhereExpressions {{ get; set; }} = new List<Expression<Func<{types}, bool>>>();

    public IList<Expression<Func<{types}, bool>>> HavingExpressions {{ get; set; }} = new List<Expression<Func<{types}, bool>>>();

    public IList<Expression> GroupByExpressions {{ get; set; }} = new List<Expression>();

    public IList<Tuple<Expression, ListSortDirection>> OrderByExpressions {{ get; set; }} = new List<Tuple<Expression, ListSortDirection>>();

                ");

                if (i > 1) {
                    // join expressions only apply when you have more than 1 table
                    var previousTypes = GetTypes(i - 1);
                    sqlFromDefinitionClassBuilder.Append(
                        $@"
    public JoinType JoinType {{ get; set; }}

    public Expression<Func<{types}, bool>> JoinExpression {{ get; set; }}

    public SqlFromDefinition<{previousTypes}> PreviousFromDefinition {{ get; set; }}

    public SqlFromDefinition(SqlFromDefinition<{previousTypes}> previousFromDefinition, JoinType joinType) {{
        this.PreviousFromDefinition = previousFromDefinition;
        this.JoinType = joinType;
    }}

    public SqlFromDefinition(SqlFromDefinition<{previousTypes}> previousFromDefinition, JoinType joinType, Expression<Func<{types}, bool>> joinExpression) {{
        this.PreviousFromDefinition = previousFromDefinition;
        this.JoinType = joinType;
        this.JoinExpression = joinExpression;
    }}
");
                } else {
                    sqlFromDefinitionClassBuilder.Append($@"
    public ISession Session {{ get; set; }}

    public SqlFromDefinition(ISession session) {{
        this.Session = session;
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
    return new SqlFromDefinition<{nextTypes}>(this, JoinType.{joinType});
}}
");

                        sqlFromDefinitionInterfaceBuilder.Append(
                            $@"
    ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>(Expression<Func<{nextTypes}, bool>> joinExpression);
");

                        sqlFromDefinitionClassBuilder.Append(
                            $@"
public ISqlFromDefinition<{nextTypes}> {joinType}<T{i + 1}>(Expression<Func<{nextTypes}, bool>> joinExpression) {{
    return new SqlFromDefinition<{nextTypes}>(this, JoinType.{joinType}, joinExpression);
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
    this.WhereExpressions.Add(whereExpression);
return this;
                }}");

                sqlFromDefinitionInterfaceBuilder.Append(
                    $@"
    ISqlFromDefinition<{types}> Having(Expression<Func<{types}, bool>> havingExpression);
");

                sqlFromDefinitionClassBuilder.Append(
                    $@"
public ISqlFromDefinition<{types}> Having(Expression<Func<{types}, bool>> havingExpression) {{
    this.HavingExpressions.Add(havingExpression);
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
    this.GroupByExpressions.Add(groupByExpression);
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
    this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
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
    return new SqlQuerySelection<{types}, TResult>(this, selectExpression);
}}
");
                sqlFromDefinitionInterfaceBuilder.AppendLine("}");
                sqlFromDefinitionClassBuilder.AppendLine("}");
            }

            // Sql Builder Builder
            for (var i = 1; i <= 16; i++)
            {
                var types = GetTypes(i);
                sqlBuilderBuilderBuilder.Append($@"
    public class SqlBuilderBuilder<{types}, TResult> {{
        public SqlBuilderBuilder(
            SqlFromDefinition<{types}> fromDefinition, 
            Expression<Func<{types}, TResult>> selectExpression,
            ISession session) {{

        }}

        public CommandDefinition Build() {{
            throw new NotImplementedException();
        }}
    }}
");
            }

            var sqlQuerySelectionInterfaces = sqlQuerySelectionInterfaceBuilder.ToString();
            var sqlQuerySelectionClasses = sqlQuerySelectionClassBuilder.ToString();
            var sqlFromDefinitionInterfaces = sqlFromDefinitionInterfaceBuilder.ToString();
            var sqlFromDefinitionClasses = sqlFromDefinitionClassBuilder.ToString();
            var sqlBuilderBuilderClasses = sqlBuilderBuilderBuilder.ToString();
            var code = $@"
{sqlQuerySelectionInterfaces}
{sqlQuerySelectionClasses}
{sqlFromDefinitionInterfaces}
{sqlFromDefinitionClasses}
{sqlBuilderBuilderClasses}
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