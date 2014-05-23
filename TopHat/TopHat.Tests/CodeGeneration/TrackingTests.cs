using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.CodeGeneration;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.CodeGeneration
{
    public class TrackingTests : IUseFixture<Fixtures.GenerateCodeFixture>
    {
        private IGeneratedCodeManager codeManager;

        public void SetFixture(Fixtures.GenerateCodeFixture data)
        {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void ChangeRelationshipPropertyFromNullMarksAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = new User { UserId = 1 };

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangeRelationshipPropertyToNullMarksAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Author = new User { UserId = 1 };

            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = null;

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangeRelationshipPropertyNotNullMarksAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Author = new User { UserId = 1 };

            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Author = new User { UserId = 2 };

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangePropertyMarksEntityAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.PostId = 3;

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void ChangePropertyMarksPropertyAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.PostId = 3;

            Assert.True(inspector.IsPropertyDirty(p => p.PostId));
        }

        [Fact]
        public void AddtoCollectionMarksAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.Add(new Comment());

            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void AddtoCollectionMarksPropertyAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.Add(new Comment());

            Assert.True(inspector.IsPropertyDirty(p => p.Comments));
        }

        [Fact]
        public void DeleteFromCollectionMarksPropertyAsDirty()
        {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.Comments.Add(new Comment());
            this.codeManager.TrackInstance(post);
            var inspector = new TrackedEntityInspector<Post>(post);

            post.Comments.RemoveAt(0);

            Assert.True(inspector.IsPropertyDirty(p => p.Comments));
        }
    }
}