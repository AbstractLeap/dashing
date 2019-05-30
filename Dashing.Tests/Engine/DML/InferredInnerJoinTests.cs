namespace Dashing.Tests.Engine.DML {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class InferredInnerJoinTests {
        private readonly ITestOutputHelper outputHelper;

        public InferredInnerJoinTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void WhereSimpleNullableJoinRewritten() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title != null) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var blog = result.FetchTree.Children[nameof(Post.Blog)];
            Assert.False(blog.IsFetched);
            Assert.True(blog.InferredInnerJoin);
        }

        [Fact]
        public void WhereOrKeepsLeftJoin() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title != null || p.Blog.BlogId == 2 || p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var blog = result.FetchTree.Children[nameof(Post.Blog)];
            var author = result.FetchTree.Children[nameof(Post.Author)];
            Assert.False(blog.IsFetched);
            Assert.False(blog.InferredInnerJoin);
            Assert.False(author.IsFetched);
            Assert.False(author.InferredInnerJoin);
        }

        [Fact]
        public void WhereAndInfers() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title == "Foo" && p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var blog = result.FetchTree.Children[nameof(Post.Blog)];
            var author = result.FetchTree.Children[nameof(Post.Author)];
            Assert.False(blog.IsFetched);
            Assert.True(blog.InferredInnerJoin);
            Assert.False(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void WherePkDoesntJoin() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.BlogId == 3) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            Assert.Null(result.FetchTree);
        }

        [Fact]
        public void WhereAndNestedOrDoesInfer() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.BlogId == 2 && (p.Blog.Owner.UserId == 3 || p.Content.Contains("Foo")) && p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());
            var blog = result.FetchTree.Children[nameof(Post.Blog)];
            var author = result.FetchTree.Children[nameof(Post.Author)];
            Assert.False(blog.IsFetched);
            Assert.False(blog.InferredInnerJoin);
            Assert.False(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void WhereTrueGoesToInner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.IsEnabled) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var author = result.FetchTree.Children[nameof(Post.Author)];
            Assert.False(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void WhereFalseGoesToInner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => !p.Author.IsEnabled) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var author = result.FetchTree.Children[nameof(Post.Author)];
            Assert.False(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void WhereNullCheckOnPropIsINner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title == null) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            var author = result.FetchTree.Children[nameof(Post.Blog)];
            Assert.False(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void WhereIsNullCheckOnMappedPropDoesInfer() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Owner == null) as SelectQuery<Post>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            // we expect the blog to be inner joined but then the owner prop is not fetched
            var blog = result.FetchTree.Children[nameof(Post.Blog)];
            Assert.False(blog.IsFetched);
            Assert.True(blog.InferredInnerJoin);
            Assert.Empty(blog.Children);
        }

        [Fact(Skip = "This doesn't need to pass to work, the inferred inner joins are optimisations")]
        public void WhereFetchedWhereOnPKInfers() {
            var query = GetSelectQuery<Comment>()
                        .Fetch(p => p.Post.Author)
                        .Where(p => p.Post.Author.UserId == 1) as SelectQuery<Comment>;
            var result = this.GetWhereClauseWriter()
                             .GenerateSql(query.WhereClauses, null, new AutoNamingDynamicParameters());

            // we expect the blog to be inner joined but then the owner prop is not fetched
            var post = result.FetchTree.Children[nameof(Comment.Post)];
            var author = post.Children[nameof(Post.Author)];
            Assert.True(post.IsFetched);
            Assert.True(post.InferredInnerJoin);
            Assert.True(author.IsFetched);
            Assert.True(author.InferredInnerJoin);
        }

        [Fact]
        public void SimpleNullableJoinRewritten() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title != null) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] is not null)", result.Sql);
        }

        [Fact]
        public void OrKeepsLeftJoin() {
            var query = GetSelectQuery<Post>()
                        .Take(2) // added the take so the outerjoindisjunctiontransform doesn't kick in
                            .Where(p => p.Blog.Title != null || p.Blog.BlogId == 2 || p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t left join [Users] as t_101 on t.AuthorId = t_101.UserId left join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (((t_100.[Title] is not null) or (t.[BlogId] = @l_1)) or (t_101.[HeightInMeters] > @l_2)) order by t.[PostId] offset 0 rows fetch next @take rows only", result.Sql);
        }

        [Fact]
        public void AndInfers() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title == "Foo" && p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_101 on t.AuthorId = t_101.UserId inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where ((t_100.[Title] = @l_1) and (t_101.[HeightInMeters] > @l_2))", result.Sql);
        }

        [Fact]
        public void PkDoesntJoin() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.BlogId == 3) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([BlogId] = @l_1)", result.Sql);
        }

        [Fact]
        public void AndNestedOrDoesInfer() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.BlogId == 2 && (p.Blog.Owner.UserId == 3 || p.Content.Contains("Foo")) && p.Author.HeightInMeters > 3) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_101 on t.AuthorId = t_101.UserId left join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (((t.[BlogId] = @l_1) and ((t_100.[OwnerId] = @l_2) or t.[Content] like @l_3)) and (t_101.[HeightInMeters] > @l_4))", result.Sql);
        }

        [Fact]
        public void TrueGoesToInner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.IsEnabled) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where t_100.[IsEnabled] = 1", result.Sql);
        }

        [Fact]
        public void FalseGoesToInner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => !p.Author.IsEnabled) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where t_100.[IsEnabled] = 0", result.Sql);
        }

        [Fact]
        public void NullCheckOnPropIsINner() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Title == null) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] is null)", result.Sql);
        }

        [Fact]
        public void IsNullCheckOnMappedPropDoesInfer() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Blog.Owner == null) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[OwnerId] is null)", result.Sql);
        }

        private void AssertNoInference(FetchNode fetchNode) {
            if (fetchNode == null) {
                return;
            }

            Assert.False(fetchNode.InferredInnerJoin);
            foreach (var fetchNodeChild in fetchNode.Children) {
                this.AssertNoInference(fetchNodeChild.Value);
            }
        }

        private SelectWriter GetSql2012Writer(IConfiguration configuration = null) {
            if (configuration == null) {
                configuration = new CustomConfig();
            }

            return new SelectWriter(new SqlServer2012Dialect(), configuration);
        }

        private WhereClauseWriter GetWhereClauseWriter(IConfiguration configuration = null) {
            if (configuration == null) {
                configuration = new CustomConfig();
            }

            return new WhereClauseWriter(new SqlServer2012Dialect(), configuration);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new() {
            return new SelectQuery<T>(new Mock<ISelectQueryExecutor>().Object);
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}