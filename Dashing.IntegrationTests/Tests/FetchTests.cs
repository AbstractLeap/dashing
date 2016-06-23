namespace Dashing.IntegrationTests.Tests {
    using System.Linq;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More;

    using Xunit;

    public class FetchTests {
        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void ExecuteSimpleQuery(TestSessionWrapper wrapper) {
            var blogs = wrapper.Session.Query<Blog>().ToList();
            Assert.NotEmpty(blogs);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void SimpleFetchWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>().Fetch(p => p.Blog);
            Assert.NotNull(posts.First().Blog.Title);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void MultipleFetchParentWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<PostTag>().Fetch(p => p.Post).Fetch(p => p.Tag).OrderBy(p => p.PostTagId).ToList();
            Assert.NotNull(posts.First().Post.Title);
            Assert.NotNull(posts.First().Tag.Content);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void NestedFetchWorks(TestSessionWrapper wrapper) {
            var comment = wrapper.Session.Query<Comment>().Fetch(c => c.Post.Blog).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void MultipleFetchWithNestedWorks(TestSessionWrapper wrapper) {
            var comment = wrapper.Session.Query<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
            Assert.NotNull(comment.First().User.Username);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void NullableFetchReturnsNull(TestSessionWrapper wrapper) {
            var comment = wrapper.Session.Query<Comment>().Fetch(c => c.User).Where(c => c.Content == "Nullable User Content");
            Assert.Null(comment.First().User);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void NullableTripleFetchDoesNotThrow(TestSessionWrapper wrapper) {
            var comment = wrapper.Session.Query<PostTag>().Fetch(c => c.Post.Blog).Where(t => t.Tag.Content == "Null Post Tag");
            Assert.Null(comment.First().Post);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void FetchWithNonFetchedWhere(TestSessionWrapper wrapper) {
            var comment = wrapper.Session.Query<Comment>().Fetch(c => c.Post.Blog).Where(c => c.User.EmailAddress == "foo");
            Assert.Null(comment.FirstOrDefault());
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void MultipleFetchManyWithNonRootAndThenFetchWorks(TestSessionWrapper wrapper) {
            var responses =
                wrapper.Session.Query<QuestionnaireResponse>()
                       .Where(qr => qr.Questionnaire.QuestionnaireId == 1)
                       .Fetch(qr => qr.Questionnaire)
                       .FetchMany(qr => qr.Responses)
                       .ThenFetch(qrr => qrr.Question)
                       .FetchMany(qr => qr.Booking.Beds)
                       .ThenFetch(b => b.RoomSlot.Room)
                       .ToArray();
            Assert.Equal(1, responses.Length);
            Assert.Equal(1, responses.First().Booking.Beds.Count);
        }
    }
}