﻿namespace Dashing.Tests.Engine.DML {
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class SelectWriterTests {
        [Fact]
        public void SimpleQueryBuilds() {
            var dialect = new SqlServerDialect();
            var selectWriter = new SelectWriter(dialect, MakeConfig());
            var sql = selectWriter.GenerateSql(new SelectQuery<User>(new Mock<ISelectQueryExecutor>().Object));

            Assert.NotNull(sql);
        }

        [Fact]
        public void FetchTest() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId",
                sql.Sql);
        }

        [Fact]
        public void SimpleFetchReturnsGoodSignature() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("1SE", sql.FetchTree.FetchSignature);
        }

        [Fact]
        public void SimpleFetchReturnsGoodSplitOn() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("UserId", sql.FetchTree.SplitOn);
        }

        [Fact]
        public void NestedFetchTest() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[DoNotMap], t_2.[BlogId], t_2.[Title], t_2.[CreateDate], t_2.[Description] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Blogs] as t_2 on t_1.BlogId = t_2.BlogId",
                sql.Sql);
        }

        [Fact]
        public void NestedFetchGetsGoodSignature() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("2S2SEE", sql.FetchTree.FetchSignature);
        }

        [Fact]
        public void NestedFetchTestGetsGoodSpliton() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("PostId,BlogId", sql.FetchTree.SplitOn);
        }

        [Fact]
        public void NestedMultipleFetchTest() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[DoNotMap], t_2.[BlogId], t_2.[Title], t_2.[CreateDate], t_2.[Description], t_3.[UserId], t_3.[Username], t_3.[EmailAddress], t_3.[Password], t_3.[IsEnabled], t_3.[HeightInMeters] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Blogs] as t_2 on t_1.BlogId = t_2.BlogId left join [Users] as t_3 on t.UserId = t_3.UserId",
                sql.Sql);
        }

        [Fact]
        public void NestedMultipleFetchGetsGoodSignature() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("2S2SEE3SE", sql.FetchTree.FetchSignature);
        }

        [Fact]
        public void NestedMultipleFetchTestGetsGoodSpliton() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal("PostId,BlogId,UserId", sql.FetchTree.SplitOn);
        }

        [Fact]
        public void WhereIdPlus1() {
            var post = new Post { PostId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == post.PostId + 2);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal(3, sql.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereId() {
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereClosureConstantAccess() {
            var o = new Post { PostId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == o.PostId);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereEntityEquals() {
            var o = new Post { PostId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p == o);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereAssociatedEntityEquals() {
            var blog = new Blog { BlogId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog == blog);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([BlogId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereAssociatedEntityEqualsAssociatedEntity() {
            var o = new Post { PostId = 1, Blog = new Blog { BlogId = 1 } };
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog == o.Blog);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([BlogId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereEnumerableContains() {
            var ids = new[] { 1, 2, 3 };
            var query = this.GetSelectQuery<Post>().Where(p => ids.Contains(p.PostId));
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where [PostId] in @l_1", sql.Sql);
        }

        [Fact]
        public void NonFetchedRelationship() {
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog.Title == "Boo");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t left join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_1)",
                sql.Sql);
        }

        private class Foo {
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
                Justification = "Reviewed. We want to test all cases, not just best practice.")]
            public readonly int Id = 1;
        }

        [Fact]
        public void WhereNonMappedClosureConstantAccess() {
            var foo = new Foo();
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == foo.Id);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereOnFetchedProperty() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t_1.[Username] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WhereOnFetchedPrimaryKey() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.UserId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t.[AuthorId] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WhereOnPrimaryKeyAndFetchSomethingElse() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post).Where(c => c.User.UserId == 2);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId where (t.[UserId] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WhereOnDeepFetchedPrimaryKey() {
            var query = this.GetSelectQuery<Comment>().Fetch(p => p.Post.Author).Where(p => p.Post.Author.UserId == 1);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[BlogId], t_1.[DoNotMap], t_2.[UserId], t_2.[Username], t_2.[EmailAddress], t_2.[Password], t_2.[IsEnabled], t_2.[HeightInMeters] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_2 on t_1.AuthorId = t_2.UserId where (t_1.[AuthorId] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WhereOnDeepDetchedProperty() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Author).Where(c => c.Post.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[BlogId], t_1.[DoNotMap], t_2.[UserId], t_2.[Username], t_2.[EmailAddress], t_2.[Password], t_2.[IsEnabled], t_2.[HeightInMeters] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_2 on t_1.AuthorId = t_2.UserId where (t_2.[Username] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void FetchWithNonFetchWhereClause() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post).Where(c => c.User.EmailAddress == "blah");
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_100 on t.UserId = t_100.UserId where (t_100.[EmailAddress] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void SimpleOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Rating);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Rating] asc", sql.Sql);
        }

        [Fact]
        public void WhereIdGetsGoodParams() {
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal(1, sql.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void IgnoreNotReturned() {
            var query = this.GetSelectQuery<Post>();
            var selectQuery = query;
            var sql = this.GetWriter(true).GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId] from [Posts]", sql.Sql);
        }

        [Fact]
        public void IgnoreNotReturnedOnFetch() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter(true).GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId",
                sql.Sql);
        }

        [Fact]
        public void TakeWorksSql() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select top (@take) [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId]", sql.Sql);
        }

        [Fact]
        public void TakeWorksSql2012WithoutOrder() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId] offset 0 rows fetch next @take rows only", sql.Sql);
        }

        [Fact]
        public void TakeWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset 0 rows fetch next @take rows only",
                sql.Sql);
        }

        [Fact]
        public void TakeWorksMySql() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` order by `PostId` limit @take", sql.Sql);
        }

        [Fact]
        public void SkipWorksSql() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select * from (select ROW_NUMBER() OVER ( order by [PostId]) as RowNum, [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId]) as pagetable where pagetable.RowNum between @skip + 1 and 18446744073709551615 order by pagetable.RowNum",
                sql.Sql);
        }

        [Fact]
        public void SkipWorksSql2012WithoutOrder() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId] offset @skip rows",
                sql.Sql);
        }

        [Fact]
        public void SkipWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset @skip rows",
                sql.Sql);
        }

        [Fact]
        public void SkipWorksMySql() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` order by `PostId` limit @skip, 18446744073709551615",
                sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql() {
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select * from (select ROW_NUMBER() OVER ( order by [PostId]) as RowNum, [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId]) as pagetable where pagetable.RowNum between @skip + 1 and @skip + @take order by pagetable.RowNum",
                sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql2012WithoutOrder() {
            // assemble
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;

            // act
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);

            // assert
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId] offset @skip rows fetch next @take rows only",
                sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset @skip rows fetch next @take rows only",
                sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksMySql() {
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` order by `PostId` limit @skip, @take",
                sql.Sql);
        }

        [Fact]
        public void CollectionThenFetch() {
            var query = this.GetSelectQuery<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap], t_1.[CommentId], t_1.[Content], t_1.[PostId], t_1.[CommentDate], t_2.[UserId], t_2.[Username], t_2.[EmailAddress], t_2.[Password], t_2.[IsEnabled], t_2.[HeightInMeters] from [Posts] as t left join [Comments] as t_1 on t.PostId = t_1.PostId left join [Users] as t_2 on t_1.UserId = t_2.UserId order by t.[PostId]",
                sql.Sql);
        }

        [Fact]
        public void MultipleCollectionAtRoot() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Tags).Fetch(p => p.Comments);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select i.[PostIdt] as [PostId], i.[Titlet] as [Title], i.[Contentt] as [Content], i.[Ratingt] as [Rating], i.[AuthorIdt] as [AuthorId], i.[BlogIdt] as [BlogId], i.[DoNotMapt] as [DoNotMap], i.[CommentIdt_2] as [CommentId], i.[Contentt_2] as [Content], i.[PostIdt_2] as [PostId], i.[UserIdt_2] as [UserId], i.[CommentDatet_2] as [CommentDate], i.[PostTagIdt_1] as [PostTagId], i.[PostIdt_1] as [PostId], i.[ElTagIdt_1] as [ElTagId] from (select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[BlogId] as [BlogIdt], t.[DoNotMap] as [DoNotMapt], t_2.[CommentId] as [CommentIdt_2], t_2.[Content] as [Contentt_2], t_2.[PostId] as [PostIdt_2], t_2.[UserId] as [UserIdt_2], t_2.[CommentDate] as [CommentDatet_2], null as PostTagIdt_1, null as PostIdt_1, null as ElTagIdt_1 from [Posts] as t left join [Comments] as t_2 on t.PostId = t_2.PostId union all select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[BlogId] as [BlogIdt], t.[DoNotMap] as [DoNotMapt], null as CommentIdt_2, null as Contentt_2, null as PostIdt_2, null as UserIdt_2, null as CommentDatet_2, t_1.[PostTagId] as [PostTagIdt_1], t_1.[PostId] as [PostIdt_1], t_1.[ElTagId] as [ElTagIdt_1] from [Posts] as t left join [PostTags] as t_1 on t.PostId = t_1.PostId) as i order by i.[PostIdt]",
                sql.Sql);
        }

        [Fact]
        public void MultipleChainedCollection() {
            var query = this.GetSelectQuery<Blog>().FetchMany(p => p.Posts).ThenFetch(p => p.Comments);
            var selectQuery = query as SelectQuery<Blog>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[BlogId], t.[Title], t.[CreateDate], t.[Description], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap], t_2.[CommentId], t_2.[Content], t_2.[PostId], t_2.[UserId], t_2.[CommentDate] from [Blogs] as t left join [Posts] as t_1 on t.BlogId = t_1.BlogId left join [Comments] as t_2 on t_1.PostId = t_2.PostId order by t.[BlogId]",
                sql.Sql);
        }

        [Fact]
        public void MultipleMultipleChainedCollection() {
            var query = this.GetSelectQuery<Blog>().FetchMany(p => p.Posts).ThenFetchMany(p => p.Comments).ThenFetch(c => c.Likes);
            var selectQuery = query as SelectQuery<Blog>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[BlogId], t.[Title], t.[CreateDate], t.[Description], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap], t_2.[CommentId], t_2.[Content], t_2.[PostId], t_2.[UserId], t_2.[CommentDate], t_3.[LikeId], t_3.[UserId], t_3.[CommentId] from [Blogs] as t left join [Posts] as t_1 on t.BlogId = t_1.BlogId left join [Comments] as t_2 on t_1.PostId = t_2.PostId left join [Likes] as t_3 on t_2.CommentId = t_3.CommentId order by t.[BlogId]",
                sql.Sql);
        }

        [Fact]
        public void MultipleManyToMany() {
            var query = this.GetSelectQuery<Post>().FetchMany(p => p.Tags).ThenFetch(p => p.ElTag).FetchMany(p => p.DeletedTags).ThenFetch(t => t.ElTag);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select i.[PostIdt] as [PostId], i.[Titlet] as [Title], i.[Contentt] as [Content], i.[Ratingt] as [Rating], i.[AuthorIdt] as [AuthorId], i.[BlogIdt] as [BlogId], i.[DoNotMapt] as [DoNotMap], i.[PostTagIdt_3] as [PostTagId], i.[PostIdt_3] as [PostId], i.[TagIdt_4] as [TagId], i.[Contentt_4] as [Content], i.[PostTagIdt_1] as [PostTagId], i.[PostIdt_1] as [PostId], i.[TagIdt_2] as [TagId], i.[Contentt_2] as [Content] from (select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[BlogId] as [BlogIdt], t.[DoNotMap] as [DoNotMapt], t_3.[PostTagId] as [PostTagIdt_3], t_3.[PostId] as [PostIdt_3], t_4.[TagId] as [TagIdt_4], t_4.[Content] as [Contentt_4], null as PostTagIdt_1, null as PostIdt_1, null as TagIdt_2, null as Contentt_2 from [Posts] as t left join [PostTags] as t_3 on t.PostId = t_3.PostId left join [Tags] as t_4 on t_3.ElTagId = t_4.TagId union all select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[BlogId] as [BlogIdt], t.[DoNotMap] as [DoNotMapt], null as PostTagIdt_3, null as PostIdt_3, null as TagIdt_4, null as Contentt_4, t_1.[PostTagId] as [PostTagIdt_1], t_1.[PostId] as [PostIdt_1], t_2.[TagId] as [TagIdt_2], t_2.[Content] as [Contentt_2] from [Posts] as t left join [PostTags] as t_1 on t.PostId = t_1.PostId left join [Tags] as t_2 on t_1.ElTagId = t_2.TagId) as i order by i.[PostIdt]",
                sql.Sql);
        }

        [Fact]
        public void ManyToOneAndMultipleManyToMany() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Blog).FetchMany(p => p.Tags).ThenFetch(p => p.ElTag).FetchMany(p => p.DeletedTags).ThenFetch(t => t.ElTag);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select i.[PostIdt] as [PostId], i.[Titlet] as [Title], i.[Contentt] as [Content], i.[Ratingt] as [Rating], i.[AuthorIdt] as [AuthorId], i.[DoNotMapt] as [DoNotMap], i.[BlogIdt_1] as [BlogId], i.[Titlet_1] as [Title], i.[CreateDatet_1] as [CreateDate], i.[Descriptiont_1] as [Description], i.[PostTagIdt_4] as [PostTagId], i.[PostIdt_4] as [PostId], i.[TagIdt_5] as [TagId], i.[Contentt_5] as [Content], i.[PostTagIdt_2] as [PostTagId], i.[PostIdt_2] as [PostId], i.[TagIdt_3] as [TagId], i.[Contentt_3] as [Content] from (select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[DoNotMap] as [DoNotMapt], t_1.[BlogId] as [BlogIdt_1], t_1.[Title] as [Titlet_1], t_1.[CreateDate] as [CreateDatet_1], t_1.[Description] as [Descriptiont_1], t_4.[PostTagId] as [PostTagIdt_4], t_4.[PostId] as [PostIdt_4], t_5.[TagId] as [TagIdt_5], t_5.[Content] as [Contentt_5], null as PostTagIdt_2, null as PostIdt_2, null as TagIdt_3, null as Contentt_3 from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId left join [PostTags] as t_4 on t.PostId = t_4.PostId left join [Tags] as t_5 on t_4.ElTagId = t_5.TagId union all select t.[PostId] as [PostIdt], t.[Title] as [Titlet], t.[Content] as [Contentt], t.[Rating] as [Ratingt], t.[AuthorId] as [AuthorIdt], t.[DoNotMap] as [DoNotMapt], t_1.[BlogId] as [BlogIdt_1], t_1.[Title] as [Titlet_1], t_1.[CreateDate] as [CreateDatet_1], t_1.[Description] as [Descriptiont_1], null as PostTagIdt_4, null as PostIdt_4, null as TagIdt_5, null as Contentt_5, t_2.[PostTagId] as [PostTagIdt_2], t_2.[PostId] as [PostIdt_2], t_3.[TagId] as [TagIdt_3], t_3.[Content] as [Contentt_3] from [Posts] as t left join [Blogs] as t_1 on t.BlogId = t_1.BlogId left join [PostTags] as t_2 on t.PostId = t_2.PostId left join [Tags] as t_3 on t_2.ElTagId = t_3.TagId) as i order by i.[PostIdt]",
                sql.Sql);
        }

        [Fact]
        public void CollectionFetchWithTake() {
            var query = this.GetSelectQuery<Blog>().Fetch(p => p.Posts).Take(10);
            var selectQuery = query as SelectQuery<Blog>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select i.[BlogId], i.[Title], i.[CreateDate], i.[Description], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from (select t.[BlogId], t.[Title], t.[CreateDate], t.[Description] from [Blogs] as t order by t.[BlogId] offset 0 rows fetch next @take rows only) as i left join [Posts] as t_1 on i.BlogId = t_1.BlogId order by i.[BlogId]",
                sql.Sql);
        }

        [Fact]
        public void WhereOnRelationshipWithNoFetchesAliasesAll() {
            var query = this.GetSelectQuery<Post>().Where(p => p.Content == "Foo" && p.Author.EmailAddress == "Foo");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t left join [Users] as t_100 on t.AuthorId = t_100.UserId where ((t.[Content] = @l_1) and (t_100.[EmailAddress] = @l_2))",
                sql.Sql);
        }

        private SelectWriter GetWriter(bool withIgnore = false) {
            return new SelectWriter(new SqlServerDialect(), MakeConfig(withIgnore));
        }

        private SelectWriter GetMySqlWriter(bool withIgnore = false) {
            return new SelectWriter(new MySqlDialect(), MakeConfig(withIgnore));
        }

        private SelectWriter GetSql2012Writer(bool withIgnore = false) {
            return new SelectWriter(new SqlServer2012Dialect(), MakeConfig(withIgnore));
        }

        private SelectQuery<T> GetSelectQuery<T>() {
            return new SelectQuery<T>(new Mock<ISelectQueryExecutor>().Object);
        }

        private static IConfiguration MakeConfig(bool withIgnore = false) {
            if (withIgnore) {
                return new CustomConfigWithIgnore();
            }

            return new CustomConfig();
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : MockConfiguration {
            public CustomConfigWithIgnore() {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}