namespace Dashing.Tests.Engine.DML {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class ExcludeIncludeTests {
        private readonly ITestOutputHelper output;

        public ExcludeIncludeTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void ExcludeRemovesColumnFromRoot() {
            var query = this.GetSelectQuery<Post>();
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Post>()
                  .Property(p => p.Content)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts]", sql.Sql);
        }

        [Fact]
        public void ExplicitExcludeRemovesColumnFromRoot() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Exclude(p => p.Content);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t", sql.Sql);
        }

        [Fact]
        public void ExcludeRemovesColumnFromGet() {
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Post>()
                  .Property(p => p.Content)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateGetSql<Post, int>(1);
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where PostId = @Id", sql.Sql);
        }

        [Fact]
        public void IncludeExcludedReaddsColumnFromRoot() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Include(p => p.Content);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Post>()
                  .Property(p => p.Content)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap], t.[Content] from [Posts] as t", sql.Sql);
        }

        [Fact]
        public void IncludeAllReaddsColumnFromRoot() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .IncludeAll();
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Post>()
                  .Property(p => p.Content)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts]", sql.Sql);
        }

        [Fact]
        public void ExcludeRemovesFetchedCollectionColumn() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Fetch(p => p.Comments);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Comment>()
                  .Property(c => c.Content)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap], t_1.[CommentId], t_1.[PostId], t_1.[UserId], t_1.[CommentDate] from [Posts] as t left join [Comments] as t_1 on t.PostId = t_1.PostId order by t.[PostId]", sql.Sql);
        }

        [Fact]
        public void ExcludeRemovesFetchedParentColumn() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Fetch(p => p.Blog);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Blog>()
                  .Property(b => b.Description)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[OwnerId] from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId", sql.Sql);
        }

        [Fact]
        public void ExplicitExcludeRemovesFetchedParentColumn() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Fetch(p => p.Blog)
                                               .Exclude(p => p.Blog.Description);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[OwnerId] from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId", sql.Sql);
        }

        [Fact]
        public void IncludeExcludedFetchedParentReaddsColumn() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Fetch(p => p.Blog)
                                               .Include(p => p.Blog.Description);
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Blog>()
                  .Property(b => b.Description)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[OwnerId], t_1.[Description] from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId", sql.Sql);
        }

        [Fact]
        public void IncludeAllReaddsColumn() {
            var query = (SelectQuery<Post>)this.GetSelectQuery<Post>()
                                               .Fetch(p => p.Blog)
                                               .IncludeAll();
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            config.Setup<Blog>()
                  .Property(b => b.Description)
                  .ExcludeByDefault();
            var writer = new SelectWriter(new SqlServerDialect(), config);
            var sql = writer.GenerateSql(query, new AutoNamingDynamicParameters());
            this.output.WriteLine(sql.Sql);
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[Description], t_1.[OwnerId] from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId", sql.Sql);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new() {
            return new SelectQuery<T>(new Mock<ISelectQueryExecutor>().Object);
        }
    }
}