using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;
using Cg = TopHat.CodeGeneration;

namespace TopHat.Tests.CodeGeneration
{
    public class ForeignKeyTests : IUseFixture<Fixtures.GenerateCodeFixture>
    {
        private Cg.IGeneratedCodeManager codeManager;

        public void SetFixture(Fixtures.GenerateCodeFixture data)
        {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void NullReferenceReturnsObjectIfColumnNotNull()
        {
            // generate Post object without Author but with AuthorId
            var post = GetPostFKWithAuthorId(3);

            Assert.Equal(3, post.Author.UserId);
        }

        [Fact]
        public void NullReferenceReturnsObjectIfColumnNotNullOnTrackedEntity()
        {
            var post = this.GetPostTrackingWithAuthorId(3);
            Assert.Equal(3, post.Author.UserId);
        }

        [Fact]
        public void NullReferenceReturnsNullIfColumnNull()
        {
            var post = GetPostFKWithoutAuthorId();

            Assert.Null(post.Author);
        }

        [Fact]
        public void NullReferenceReturnsNullIfColumnNullOnTrackedEntity()
        {
            var post = this.GetPostTrackingWithoutAuthorId();

            Assert.Null(post.Author);
        }

        [Fact]
        public void SecondGetReturnsSameObject()
        {
            var post = this.GetPostFKWithAuthorId(3);
            var author = post.Author;
            var author2 = post.Author;

            Assert.Same(author, author2);
        }

        [Fact]
        public void SetObjectReturnsObject()
        {
            var post = this.GetPostFKWithAuthorId(3);
            var user = new User { UserId = 1 };
            post.Author = user;

            Assert.Same(post.Author, user);
        }

        private Post GetPostTrackingWithoutAuthorId()
        {
            return this.codeManager.CreateTrackingInstance<Post>();
        }

        private Post GetPostFKWithoutAuthorId()
        {
            return this.codeManager.CreateForeignKeyInstance<Post>();
        }

        private Post GetPostFKWithAuthorId(int authorId)
        {
            var postFkType = this.codeManager.GetForeignKeyType<Post>();
            var postFk = this.codeManager.CreateForeignKeyInstance<Post>();
            postFkType.GetProperty("AuthorId").SetValue(postFk, authorId);
            return postFk as Post;
        }

        private Post GetPostTrackingWithAuthorId(int authorId)
        {
            var postTrackingType = this.codeManager.GetTrackingType<Post>();
            var postTracking = this.codeManager.CreateTrackingInstance<Post>();
            postTrackingType.GetProperty("AuthorId").SetValue(postTracking, authorId);
            return postTracking as Post;
        }
    }
}