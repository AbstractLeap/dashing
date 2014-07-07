namespace Dashing.Tests.CodeGeneration {
    using Dashing.CodeGeneration;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class ForeignKeyTests : IUseFixture<GenerateCodeFixture> {
        private IGeneratedCodeManager codeManager;

        public void SetFixture(GenerateCodeFixture data) {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void NullReferenceReturnsObjectIfColumnNotNull() {
            // generate Post object without Author but with AuthorId
            var post = this.GetPostFKWithAuthorId(3);

            Assert.Equal(3, post.Author.UserId);
        }

        [Fact]
        public void NullReferenceReturnsObjectIfColumnNotNullOnTrackedEntity() {
            var post = this.GetPostTrackingWithAuthorId(3);
            Assert.Equal(3, post.Author.UserId);
        }

        [Fact]
        public void NullReferenceReturnsNullIfColumnNull() {
            var post = this.GetPostFKWithoutAuthorId();

            Assert.Null(post.Author);
        }

        [Fact]
        public void NullReferenceReturnsNullIfColumnNullOnTrackedEntity() {
            var post = this.GetPostTrackingWithoutAuthorId();

            Assert.Null(post.Author);
        }

        [Fact]
        public void SecondGetReturnsSameObject() {
            var post = this.GetPostFKWithAuthorId(3);
            var author = post.Author;
            var author2 = post.Author;

            Assert.Same(author, author2);
        }

        [Fact]
        public void SetObjectReturnsObject() {
            var post = this.GetPostFKWithAuthorId(3);
            var user = new User { UserId = 1 };
            post.Author = user;

            Assert.Same(post.Author, user);
        }

        private Post GetPostTrackingWithoutAuthorId() {
            return this.codeManager.CreateTrackingInstance<Post>();
        }

        private Post GetPostFKWithoutAuthorId() {
            return this.codeManager.CreateForeignKeyInstance<Post>();
        }

        private Post GetPostFKWithAuthorId(int authorId) {
            var postFkType = this.codeManager.GetForeignKeyType(typeof(Post));
            var postFk = this.codeManager.CreateForeignKeyInstance<Post>();
            postFkType.GetProperty("AuthorId").SetValue(postFk, authorId);
            return postFk;
        }

        private Post GetPostTrackingWithAuthorId(int authorId) {
            var postTrackingType = this.codeManager.GetTrackingType(typeof(Post));
            var postTracking = this.codeManager.CreateTrackingInstance<Post>();
            postTrackingType.GetProperty("AuthorId").SetValue(postTracking, authorId);
            return postTracking;
        }
    }
}