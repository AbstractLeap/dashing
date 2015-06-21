namespace Dashing.Tests.CodeGeneration {
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class TrackingTests : IClassFixture<GenerateCodeFixture> {
        private readonly IGeneratedCodeManager codeManager;

        public TrackingTests(GenerateCodeFixture data) {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void AddTrackingWorks() {
            var post = new Post { PostId = 1 };
            var trackedPost = this.codeManager.CreateTrackingInstance(post);
            Assert.IsType(this.codeManager.GetTrackingType(typeof(Post)), trackedPost);
        }

        [Fact]
        public void AddTrackingReturnsEntityIfAlreadyTracked() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            var trackedPost = this.codeManager.CreateTrackingInstance(post);
            Assert.Same(post, trackedPost);
        }

        [Fact]
        public void AddTrackingDoesNotMakeDirty() {
            var post = new Post { PostId = 1, Author = new User { UserId = 1 }, Title = "Blah" };
            var trackedPost = this.codeManager.CreateTrackingInstance(post);
            Assert.Empty(((ITrackedEntity)trackedPost).GetDirtyProperties());
        }

        [Fact]
        public void AddTrackingCopiesValues() {
            var post = new Post { PostId = 1, Author = new User { UserId = 1 }, Title = "Blah" };
            var trackedPost = this.codeManager.CreateTrackingInstance(post);
            Assert.Equal(1, trackedPost.PostId);
            Assert.Equal(1, trackedPost.Author.UserId);
            Assert.Equal("Blah", trackedPost.Title);
        }

        [Fact]
        public void AddTrackingStartsTracking() {
            var post = new Post { PostId = 1, Author = new User { UserId = 1 }, Title = "Blah" };
            var trackedPost = this.codeManager.CreateTrackingInstance(post);
            Assert.True(((ITrackedEntity)trackedPost).IsTrackingEnabled());
        }

        [Fact]
        public void ChangeRelationshipPropertyFromNullMarksAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = new User { UserId = 1 };

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangeRelationshipPropertyToNullMarksAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Author = new User { UserId = 1 };

            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = null;

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangeRelationshipPropertyNotNullMarksAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Author = new User { UserId = 1 };

            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = new User { UserId = 2 };

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangePropertyMarksEntityAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.PostId = 3;

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangePropertyMarksPropertyAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.PostId = 3;

            Assert.True(inspector.IsPropertyDirty(p => p.PostId));
        }

        [Fact]
        public void AddtoCollectionMarksAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.Add(new Comment());

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void AddtoCollectionMarksPropertyAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.Add(new Comment());

            Assert.True(inspector.IsPropertyDirty(p => p.Comments));
        }

        [Fact]
        public void DeleteFromCollectionMarksPropertyAsDirty() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Comments.Add(new Comment());
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.RemoveAt(0);

            Assert.True(inspector.IsPropertyDirty(p => p.Comments));
        }

        [Fact]
        public void OldValueForWorks() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Title = "Foo";
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);
            post.Title = "Boo";

            Assert.Equal("Foo", inspector.OldValueFor(p => p.Title));
        }

        [Fact]
        public void NewValueForWorks() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Title = "Foo";
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);
            post.Title = "Boo";

            Assert.Equal("Boo", inspector.NewValueFor("Title"));
        }

        [Fact]
        public void OldValuesWorks() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Title = "Foo";
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);
            post.Title = "Boo";

            Assert.Equal("Foo", inspector.OldValues.First().Value);
        }
    }
}