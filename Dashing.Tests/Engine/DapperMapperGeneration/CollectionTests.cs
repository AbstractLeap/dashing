namespace Dashing.Tests.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class CollectionTests {
        [Fact]
        public void SingleRowCollectionTestForJames() {
            var funcFac = GenerateSingleMapperWithFetch();
            var post1 = new Post { PostId = 1 };
            var comment1 = new Comment { CommentId = 1 };
            var blog1 = new Blog { BlogId = 1 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, blog1, comment1 });
            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(1, results[0].Blog.BlogId);
        }

        [Fact]
        public void SingleCollectionWorks() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(2, results[0].Comments.Last().CommentId);
            Assert.Equal(3, results[1].Comments.First().CommentId);
        }

        [Fact]
        public void SingleCollectionHasTrackingEnabled() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.True(((ITrackedEntity)results[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Comments.First()).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Comments.ElementAt(1)).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[1]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[1].Comments.First()).IsTrackingEnabled());
        }

        [Fact]
        public void SingleCollectionHasTrackingEnabledLast() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.False(((ITrackedEntity)results[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Comments.First()).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Comments.ElementAt(1)).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[1]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[1].Comments.First()).GetDirtyProperties().Any());
        }

        [Fact]
        public void SingleCollectionAwkwardObjectWorks() {
            var funcFac = GenerateSingleAwkwardMapper();
            var post1 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 1 };
            var post2 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 2 };
            var comment1 = new CommentTwo { CommentTwoId = 1 };
            var comment2 = new CommentTwo { CommentTwoId = 2 };
            var comment3 = new CommentTwo { CommentTwoId = 3 };
            PostWithoutCollectionInitializerInConstructor currentRoot = null;
            IList<PostWithoutCollectionInitializerInConstructor> results = new List<PostWithoutCollectionInitializerInConstructor>();
            var func = (Func<object[], PostWithoutCollectionInitializerInConstructor>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.Equal(1, results[0].Comments.First().CommentTwoId);
            Assert.Equal(2, results[0].Comments.Last().CommentTwoId);
            Assert.Equal(3, results[1].Comments.First().CommentTwoId);
        }

        [Fact]
        public void ThenFetchWorks() {
            var funcFac = GenerateThenFetchMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1, user1 });
            func(new object[] { post1, comment2, user2 });
            func(new object[] { post2, comment3, user1 });
            Assert.Equal(1, results[0].Comments.First().User.UserId);
            Assert.Equal(2, results[0].Comments.Last().User.UserId);
            Assert.Equal(1, results[1].Comments.First().User.UserId);
        }

        [Fact]
        public void ThenFetchWorksTrackingEnabled() {
            var funcFac = GenerateThenFetchMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1, user1 });
            func(new object[] { post1, comment2, user2 });
            func(new object[] { post2, comment3, user1 });
            Assert.True(((ITrackedEntity)results[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Comments.First()).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Comments.First().User).IsTrackingEnabled());
        }

        [Fact]
        public void ThenFetchWorksTrackingEnabledLast() {
            var funcFac = GenerateThenFetchMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1, user1 });
            func(new object[] { post1, comment2, user2 });
            func(new object[] { post2, comment3, user1 });
            Assert.False(((ITrackedEntity)results[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Comments.First()).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Comments.First().User).GetDirtyProperties().Any());
        }

        private static Delegate GenerateThenFetchMapper() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Comments).ThenFetch(c => c.User) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());

            var mapper = new SingleCollectionMapperGenerator(config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree);
            return func.Item1;
        }

        [Fact]
        public void MultiCollectionWorks() {
            var funcFac = GenerateMultiMapper();

            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var postTag1 = new PostTag { PostTagId = 1 };
            var postTag2 = new PostTag { PostTagId = 2 };
            var postTag3 = new PostTag { PostTagId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            IDictionary<int, Comment> dict0 = new Dictionary<int, Comment>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            IDictionary<int, PostTag> dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();

            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1);
            func(new object[] { post1, comment1, postTag1 });
            func(new object[] { post1, comment2, postTag1 });
            func(new object[] { post2, comment3, postTag2 });
            func(new object[] { post2, comment3, postTag3 });

            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(2, results[0].Comments.Last().CommentId);
            Assert.Equal(2, results[0].Comments.Count);

            Assert.Equal(3, results[1].Comments.First().CommentId);
            Assert.Equal(1, results[1].Comments.Count);

            Assert.Equal(1, results[0].Tags.First().PostTagId);
            Assert.Equal(1, results[0].Tags.Count);

            Assert.Equal(2, results[1].Tags.First().PostTagId);
            Assert.Equal(3, results[1].Tags.Last().PostTagId);
            Assert.Equal(2, results[1].Tags.Count);
        }

        [Fact]
        public void FetchNonRootCollectionWorks() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostTag>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Post.Comments).Take(1) as SelectQuery<PostTag>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new SingleCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateCollectionMapper<PostTag>(result.FetchTree).Item1;

            var tag1 = new PostTag { PostTagId = 1 };
            var tag2 = new PostTag { PostTagId = 2 };
            var tag3 = new PostTag { PostTagId = 3 };
            var post1 = new Post { PostId = 1, Title = "Foo" };
            var anotherPost1 = new Post { PostId = 1, Title = "Foo" };
            var post2 = new Post { PostId = 2, Title = "Foo" };
            var post3 = new Post { PostId = 3, Title = "Foo" };
            var post4 = new Post { PostId = 4, Title = "Foo" };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var comment4 = new Comment { CommentId = 4 };
            var comment5 = new Comment { CommentId = 5 };
            var comment6 = new Comment { CommentId = 6 };

            PostTag currentRoot = null;
            IList<PostTag> results = new List<PostTag>();
            var func = (Func<object[], PostTag>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { tag1, post1, comment1 });
            func(new object[] { tag1, post1, comment2 });
            func(new object[] { tag1, post1, comment3 });
            func(new object[] { tag2, anotherPost1, comment1 });
            func(new object[] { tag2, anotherPost1, comment2 });
            func(new object[] { tag2, anotherPost1, comment3 });
            func(new object[] { tag3, post2, comment4 });
            func(new object[] { tag3, post2, comment5 });
            func(new object[] { tag3, post2, comment6 });

            Assert.Equal(3, results.Count);
            Assert.Equal(3, results.First().Post.Comments.Count);
            Assert.Equal(1, results.First().Post.PostId);
            Assert.Equal(3, results.ElementAt(1).Post.Comments.Count);
            Assert.Equal(1, results.ElementAt(1).Post.PostId);
            Assert.Equal(3, results.ElementAt(2).Post.Comments.Count);
            Assert.Equal(2, results.ElementAt(2).Post.PostId);
            Assert.Equal(4, results.ElementAt(2).Post.Comments.First().CommentId);
            Assert.Equal(5, results.ElementAt(2).Post.Comments.ElementAt(1).CommentId);
            Assert.Equal(6, results.ElementAt(2).Post.Comments.ElementAt(2).CommentId);
        }

        [Fact]
        public void FetchManyNonRootWorks() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostTag>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Post.Comments).ThenFetch(c => c.User) as
                SelectQuery<PostTag>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new SingleCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateCollectionMapper<PostTag>(result.FetchTree).Item1;

            // setup the scenario
            var tag1 = new PostTag { PostTagId = 1 };
            var tag2 = new PostTag { PostTagId = 2 };
            var tag3 = new PostTag { PostTagId = 3 };
            var post1 = new Post { PostId = 1, Title = "Foo" };
            var anotherPost1 = new Post { PostId = 1, Title = "Foo" };
            var post2 = new Post { PostId = 2, Title = "Foo" };
            var post3 = new Post { PostId = 3, Title = "Foo" };
            var post4 = new Post { PostId = 4, Title = "Foo" };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var comment4 = new Comment { CommentId = 4 };
            var comment5 = new Comment { CommentId = 5 };
            var comment6 = new Comment { CommentId = 6 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            var user3 = new User { UserId = 3 };
            var user4 = new User { UserId = 4 };
            var user5 = new User { UserId = 5 };

            PostTag currentRoot = null;
            IList<PostTag> results = new List<PostTag>();
            var func = (Func<object[], PostTag>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { tag1, post1, comment1, user1 });
            func(new object[] { tag1, post1, comment2, user1 });
            func(new object[] { tag1, post1, comment3, user2 });
            func(new object[] { tag2, anotherPost1, comment1, user1 });
            func(new object[] { tag2, anotherPost1, comment2, user1 });
            func(new object[] { tag2, anotherPost1, comment3, user2 });
            func(new object[] { tag3, post2, comment4, user3 });
            func(new object[] { tag3, post2, comment5, user4 });
            func(new object[] { tag3, post2, comment6, user5 });

            Assert.Equal(3, results.Count);
            Assert.Equal(3, results.First().Post.Comments.Count);
            Assert.Equal(1, results.First().Post.PostId);
            Assert.Equal(1, results.First().Post.Comments.First().User.UserId);
            Assert.Equal(1, results.First().Post.Comments.ElementAt(1).User.UserId);
            Assert.Equal(2, results.First().Post.Comments.ElementAt(2).User.UserId);
            Assert.Equal(3, results.ElementAt(1).Post.Comments.Count);
            Assert.Equal(1, results.ElementAt(1).Post.PostId);
            Assert.Equal(1, results.ElementAt(1).Post.Comments.First().User.UserId);
            Assert.Equal(1, results.ElementAt(1).Post.Comments.ElementAt(1).User.UserId);
            Assert.Equal(2, results.ElementAt(1).Post.Comments.ElementAt(2).User.UserId);
            Assert.Equal(3, results.ElementAt(2).Post.Comments.Count);
            Assert.Equal(2, results.ElementAt(2).Post.PostId);
            Assert.Equal(4, results.ElementAt(2).Post.Comments.First().CommentId);
            Assert.Equal(5, results.ElementAt(2).Post.Comments.ElementAt(1).CommentId);
            Assert.Equal(6, results.ElementAt(2).Post.Comments.ElementAt(2).CommentId);
        }

        [Fact]
        public void FetchManyNonRootTrackingEnabled() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostTag>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Post.Comments).ThenFetch(c => c.User) as
                SelectQuery<PostTag>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new SingleCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateCollectionMapper<PostTag>(result.FetchTree).Item1;

            // setup the scenario
            var tag1 = new PostTag { PostTagId = 1 };
            var tag2 = new PostTag { PostTagId = 2 };
            var tag3 = new PostTag { PostTagId = 3 };
            var post1 = new Post { PostId = 1, Title = "Foo" };
            var anotherPost1 = new Post { PostId = 1, Title = "Foo" };
            var post2 = new Post { PostId = 2, Title = "Foo" };
            var post3 = new Post { PostId = 3, Title = "Foo" };
            var post4 = new Post { PostId = 4, Title = "Foo" };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var comment4 = new Comment { CommentId = 4 };
            var comment5 = new Comment { CommentId = 5 };
            var comment6 = new Comment { CommentId = 6 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            var user3 = new User { UserId = 3 };
            var user4 = new User { UserId = 4 };
            var user5 = new User { UserId = 5 };

            PostTag currentRoot = null;
            IList<PostTag> results = new List<PostTag>();
            var func = (Func<object[], PostTag>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { tag1, post1, comment1, user1 });
            func(new object[] { tag1, post1, comment2, user1 });
            func(new object[] { tag1, post1, comment3, user2 });
            func(new object[] { tag2, anotherPost1, comment1, user1 });
            func(new object[] { tag2, anotherPost1, comment2, user1 });
            func(new object[] { tag2, anotherPost1, comment3, user2 });
            func(new object[] { tag3, post2, comment4, user3 });
            func(new object[] { tag3, post2, comment5, user4 });
            func(new object[] { tag3, post2, comment6, user5 });

            Assert.True(((ITrackedEntity)results[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Post).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Post.Comments[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Post.Comments[0].User).IsTrackingEnabled());
        }

        [Fact]
        public void FetchManyNonRootTrackingEnabledLast() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostTag>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Post.Comments).ThenFetch(c => c.User) as
                SelectQuery<PostTag>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new SingleCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateCollectionMapper<PostTag>(result.FetchTree).Item1;

            // setup the scenario
            var tag1 = new PostTag { PostTagId = 1 };
            var tag2 = new PostTag { PostTagId = 2 };
            var tag3 = new PostTag { PostTagId = 3 };
            var post1 = new Post { PostId = 1, Title = "Foo" };
            var anotherPost1 = new Post { PostId = 1, Title = "Foo" };
            var post2 = new Post { PostId = 2, Title = "Foo" };
            var post3 = new Post { PostId = 3, Title = "Foo" };
            var post4 = new Post { PostId = 4, Title = "Foo" };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var comment4 = new Comment { CommentId = 4 };
            var comment5 = new Comment { CommentId = 5 };
            var comment6 = new Comment { CommentId = 6 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            var user3 = new User { UserId = 3 };
            var user4 = new User { UserId = 4 };
            var user5 = new User { UserId = 5 };

            PostTag currentRoot = null;
            IList<PostTag> results = new List<PostTag>();
            var func = (Func<object[], PostTag>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { tag1, post1, comment1, user1 });
            func(new object[] { tag1, post1, comment2, user1 });
            func(new object[] { tag1, post1, comment3, user2 });
            func(new object[] { tag2, anotherPost1, comment1, user1 });
            func(new object[] { tag2, anotherPost1, comment2, user1 });
            func(new object[] { tag2, anotherPost1, comment3, user2 });
            func(new object[] { tag3, post2, comment4, user3 });
            func(new object[] { tag3, post2, comment5, user4 });
            func(new object[] { tag3, post2, comment6, user5 });

            Assert.False(((ITrackedEntity)results[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Post).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Post.Comments[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Post.Comments[0].User).GetDirtyProperties().Any());
        }

        [Fact]
        public void MultipleManyToManyFetchingWorks() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Tags)
                                                                              .ThenFetch(p => p.ElTag)
                                                                              .FetchMany(p => p.DeletedTags)
                                                                              .ThenFetch(t => t.ElTag) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new MultiCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Post>(result.FetchTree).Item1;

            // setup the scenario
            var post1 = new Post { PostId = 1 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var postTag1 = new PostTag { PostTagId = 1 };
            var postTag2 = new PostTag { PostTagId = 2 };
            var postTag3 = new PostTag { PostTagId = 3 };

            // act
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var dict0 = new Dictionary<int, PostTag>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();

            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1);
            func(new object[] { post1, postTag2, tag2, postTag1, tag1 });
            func(new object[] { post1, postTag3, tag3, postTag1, tag1 });

            Assert.Equal(1, results.Count);
            Assert.Equal(1, results[0].Tags.Count);
            Assert.Equal(1, results[0].Tags[0].PostTagId);
            Assert.Equal(2, results[0].DeletedTags.Count);
            Assert.Equal(2, results[0].DeletedTags[0].PostTagId);
            Assert.Equal(3, results[0].DeletedTags[1].PostTagId);
        }

        [Fact]
        public void NestedMultipleOneToManyFetchingWorks() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Blog>(new Mock<ISelectQueryExecutor>().Object).FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.Tags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.DeletedTags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(p => p.Posts)
                                                                              .ThenFetch(p => p.Author) as SelectQuery<Blog>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new MultiCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Blog>(result.FetchTree).Item1;

            // setup the scenario
            var blog1 = new Blog { BlogId = 1 };
            var blog2 = new Blog { BlogId = 2 };
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var post3 = new Post { PostId = 3 };
            var posttag1 = new PostTag { PostTagId = 1 };
            var posttag2 = new PostTag { PostTagId = 2 };
            var posttag3 = new PostTag { PostTagId = 3 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var tag4 = new Tag { TagId = 4 };
            var delPostTag1 = new PostTag { PostTagId = 3 };
            var delPostTag2 = new PostTag { PostTagId = 4 };
            var delPostTag3 = new PostTag { PostTagId = 5 };
            var author1 = new User { UserId = 1 };
            var author2 = new User { UserId = 2 };

            // act
            Blog currentRoot = null;
            IList<Blog> results = new List<Blog>();
            var dict0 = new Dictionary<int, Post>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();
            var dict2 = new Dictionary<int, PostTag>();
            var hashsetPair2 = new HashSet<Tuple<int, int>>();

            var func =
                (Func<object[], Blog>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1, dict2, hashsetPair2);
            func(new object[] { blog1, post1, author1, null, null, posttag1, tag1 });
            func(new object[] { blog1, post1, author1, null, null, posttag2, tag2 });
            func(new object[] { blog1, post2, author2, delPostTag1, tag3, null, null });
            func(new object[] { blog1, post2, author2, delPostTag2, tag4, null, null });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag3, tag3 });

            Assert.Equal(2, results.Count);
            Assert.Equal(2, results[0].Posts.Count);
            Assert.Equal(1, results[0].Posts[0].Author.UserId);
            Assert.True(results[0].Posts[0].DeletedTags == null || !results[0].Posts[0].DeletedTags.Any());
            Assert.Equal(2, results[0].Posts[0].Tags.Count);
            Assert.Equal(1, results[0].Posts[0].Tags[0].PostTagId);
            Assert.Equal(1, results[0].Posts[0].Tags[0].ElTag.TagId);
            Assert.Equal(2, results[0].Posts[0].Tags[1].PostTagId);
            Assert.Equal(2, results[0].Posts[0].Tags[1].ElTag.TagId);
            Assert.Equal(1, results[1].Posts.Count);
            Assert.Equal(1, results[1].Posts[0].Author.UserId);
            Assert.Equal(3, results[1].Posts[0].DeletedTags.Count);
            Assert.Equal(3, results[1].Posts[0].Tags.Count);
            Assert.Equal(3, results[1].Posts[0].DeletedTags[0].PostTagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[1].PostTagId);
            Assert.Equal(5, results[1].Posts[0].DeletedTags[2].PostTagId);
            Assert.Equal(3, results[1].Posts[0].DeletedTags[0].ElTag.TagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[1].ElTag.TagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[2].ElTag.TagId);
            Assert.Equal(1, results[1].Posts[0].Tags[0].PostTagId);
            Assert.Equal(2, results[1].Posts[0].Tags[1].PostTagId);
            Assert.Equal(3, results[1].Posts[0].Tags[2].PostTagId);
            Assert.Equal(1, results[1].Posts[0].Tags[0].ElTag.TagId);
            Assert.Equal(2, results[1].Posts[0].Tags[1].ElTag.TagId);
            Assert.Equal(3, results[1].Posts[0].Tags[2].ElTag.TagId);
        }

        [Fact]
        public void NestedMultipleOneToManyFetchingWorksTrackingEnabled() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Blog>(new Mock<ISelectQueryExecutor>().Object).FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.Tags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.DeletedTags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(p => p.Posts)
                                                                              .ThenFetch(p => p.Author) as SelectQuery<Blog>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new MultiCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Blog>(result.FetchTree).Item1;

            // setup the scenario
            var blog1 = new Blog { BlogId = 1 };
            var blog2 = new Blog { BlogId = 2 };
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var post3 = new Post { PostId = 3 };
            var posttag1 = new PostTag { PostTagId = 1 };
            var posttag2 = new PostTag { PostTagId = 2 };
            var posttag3 = new PostTag { PostTagId = 3 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var tag4 = new Tag { TagId = 4 };
            var delPostTag1 = new PostTag { PostTagId = 3 };
            var delPostTag2 = new PostTag { PostTagId = 4 };
            var delPostTag3 = new PostTag { PostTagId = 5 };
            var author1 = new User { UserId = 1 };
            var author2 = new User { UserId = 2 };

            // act
            Blog currentRoot = null;
            IList<Blog> results = new List<Blog>();
            var dict0 = new Dictionary<int, Post>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();
            var dict2 = new Dictionary<int, PostTag>();
            var hashsetPair2 = new HashSet<Tuple<int, int>>();

            var func =
                (Func<object[], Blog>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1, dict2, hashsetPair2);
            func(new object[] { blog1, post1, author1, null, null, posttag1, tag1 });
            func(new object[] { blog1, post1, author1, null, null, posttag2, tag2 });
            func(new object[] { blog1, post2, author2, delPostTag1, tag3, null, null });
            func(new object[] { blog1, post2, author2, delPostTag2, tag4, null, null });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag3, tag3 });

            Assert.True(((ITrackedEntity)results[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Posts[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Posts[0].Author).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Posts[0].Tags[0]).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)results[0].Posts[0].Tags[0].ElTag).IsTrackingEnabled());
        }

        [Fact]
        public void NestedMultipleOneToManyFetchingWorksTrackingEnabledLast() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Blog>(new Mock<ISelectQueryExecutor>().Object).FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.Tags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(b => b.Posts)
                                                                              .ThenFetchMany(p => p.DeletedTags)
                                                                              .ThenFetch(t => t.ElTag)
                                                                              .FetchMany(p => p.Posts)
                                                                              .ThenFetch(p => p.Author) as SelectQuery<Blog>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new MultiCollectionMapperGenerator(config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Blog>(result.FetchTree).Item1;

            // setup the scenario
            var blog1 = new Blog { BlogId = 1 };
            var blog2 = new Blog { BlogId = 2 };
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var post3 = new Post { PostId = 3 };
            var posttag1 = new PostTag { PostTagId = 1 };
            var posttag2 = new PostTag { PostTagId = 2 };
            var posttag3 = new PostTag { PostTagId = 3 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var tag4 = new Tag { TagId = 4 };
            var delPostTag1 = new PostTag { PostTagId = 3 };
            var delPostTag2 = new PostTag { PostTagId = 4 };
            var delPostTag3 = new PostTag { PostTagId = 5 };
            var author1 = new User { UserId = 1 };
            var author2 = new User { UserId = 2 };

            // act
            Blog currentRoot = null;
            IList<Blog> results = new List<Blog>();
            var dict0 = new Dictionary<int, Post>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();
            var dict2 = new Dictionary<int, PostTag>();
            var hashsetPair2 = new HashSet<Tuple<int, int>>();

            var func =
                (Func<object[], Blog>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1, dict2, hashsetPair2);
            func(new object[] { blog1, post1, author1, null, null, posttag1, tag1 });
            func(new object[] { blog1, post1, author1, null, null, posttag2, tag2 });
            func(new object[] { blog1, post2, author2, delPostTag1, tag3, null, null });
            func(new object[] { blog1, post2, author2, delPostTag2, tag4, null, null });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag3, tag3 });

            Assert.False(((ITrackedEntity)results[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Posts[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Posts[0].Author).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Posts[0].Tags[0]).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)results[0].Posts[0].Tags[0].ElTag).GetDirtyProperties().Any());
        }

        private static Delegate GenerateMultiMapper() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments).Fetch(p => p.Tags) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());

            var mapper = new MultiCollectionMapperGenerator(config);
            var func = mapper.GenerateMultiCollectionMapper<Post>(result.FetchTree);
            return func.Item1;
        }

        private static Delegate GenerateSingleMapperWithFetch() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments).Fetch(p => p.Blog) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());

            var mapper = new SingleCollectionMapperGenerator(config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree);
            return func.Item1;
        }

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());

            var mapper = new SingleCollectionMapperGenerator(config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree);
            return func.Item1;
        }

        private static Delegate GenerateSingleAwkwardMapper() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostWithoutCollectionInitializerInConstructor>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments) as
                SelectQuery<PostWithoutCollectionInitializerInConstructor>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());

            var mapper = new SingleCollectionMapperGenerator(config);
            var func = mapper.GenerateCollectionMapper<PostWithoutCollectionInitializerInConstructor>(result.FetchTree);
            return func.Item1;
        }

        private class CustomConfig : BaseConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
                this.Add<PostWithoutCollectionInitializerInConstructor>();
                this.Add<CommentTwo>();
            }
        }

        private class PostWithoutCollectionInitializerInConstructor {
            public virtual int PostWithoutCollectionInitializerInConstructorId { get; set; }

            public virtual string Name { get; set; }

            public virtual ICollection<CommentTwo> Comments { get; set; }
        }

        private class CommentTwo {
            public virtual int CommentTwoId { get; set; }

            public virtual PostWithoutCollectionInitializerInConstructor PostWithoutCollectionInitializerInConstructor { get; set; }
        }
    }
}