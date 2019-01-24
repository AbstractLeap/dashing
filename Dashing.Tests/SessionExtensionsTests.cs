namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Dashing.Tests.TestDomain;

    using Xunit;

    public class SessionExtensionsTests : IDisposable {
        private ISession session;

        public SessionExtensionsTests() {
            var config = new TestConfig();
            var database = new InMemoryDatabase(config);
            this.session = database.BeginSession();
            this.session.Insert(new Post { Title = "Foo" });
        }

        [Fact]
        public void SingleEntityInsertWorks() {
            this.session.Insert(new Post { Title = "Bar" });
            Assert.Equal(1, this.session.Query<Post>().Count(p => p.Title == "Bar"));
        }

        [Fact]
        public void ExtensionMultiEntityInsertWorks()
        {
            this.session.Insert(new Post { Title = "Bar" }, new Post { Title = "Bar" });
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Bar"));
        }

        [Fact]
        public void ArrayEntityInsertWorks()
        {
            var posts = new [] { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            this.session.Insert(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Bar"));
        }

        [Fact]
        public void ListEntityInsertWorks()
        {
            var posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            this.session.Insert(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Bar"));
        }

        [Fact]
        public void ClassListPropertyEntityInsertWorks()
        {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } } };
            this.session.Insert(parent.Posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Bar"));
        }

        [Fact]
        public void SingleEntitySaveWorks() {
            var entity = this.session.Get<Post>(1);
            entity.Title = "Bar";
            this.session.Save(entity);
            Assert.Equal(1, this.session.Query<Post>().Count(p => p.Title == "Bar"));
            Assert.Empty(this.session.Query<Post>().Where(p => p.Title == "Foo"));
        }

        [Fact]
        public void ExtensionMultiEntitySaveWorks() {
            var entity1 = this.session.Query<Post>().First();
            var entity2 = new Post { Title = "Foo" };
            this.session.Insert(entity2);
            entity1.Title = "Bar";
            entity2.Title = "Bar";
            this.session.Save(entity1, entity2);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Bar"));
            Assert.Empty(this.session.Query<Post>().Where(p => p.Title == "Foo"));
        }

        [Fact]
        public void ArrayEntitySaveWorks() {
            var posts = new[] { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            this.session.Insert(posts);
            foreach (var post in posts) {
                post.Title = "Banana";
            }

            this.session.Save(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public void ListEntitySaveWorks() {
            var posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            this.session.Insert(posts);
            foreach (var post in posts) {
                post.Title = "Banana";
            }

            this.session.Save(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public void ClassListPropertyEntitySaveWorks() {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } } };
            this.session.Insert(parent.Posts);
            foreach (var post in parent.Posts) {
                post.Title = "Banana";
            }

            this.session.Save(parent.Posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public void SingleEntityDeleteWorks() {
            var entity = this.session.Get<Post>(1);
            this.session.Delete(entity);
            Assert.Empty(this.session.Query<Post>());
        }

        [Fact]
        public void ExtensionMultiEntityDeleteWorks() {
            var entity1 = this.session.Query<Post>().First();
            var entity2 = new Post { Title = "Foo" };
            this.session.Insert(entity2);
            this.session.Delete(entity1, entity2);
            Assert.Empty(this.session.Query<Post>());
        }

        [Fact]
        public void ArrayEntityDeleteWorks() {
            var posts = new[] { new Post { Title = "Banana" }, new Post { Title = "Banana" } };
            this.session.Insert(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
            this.session.Delete(posts);
            Assert.Equal(0, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public void ListEntityDeleteWorks() {
            var posts = new List<Post> { new Post { Title = "Banana" }, new Post { Title = "Banana" } };
            this.session.Insert(posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
            this.session.Delete(posts);
            Assert.Equal(0, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public void ClassListPropertyEntityDeleteWorks() {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Banana" }, new Post { Title = "Banana" } } };
            this.session.Insert(parent.Posts);
            Assert.Equal(2, this.session.Query<Post>().Count(p => p.Title == "Banana"));
            this.session.Delete(parent.Posts);
            Assert.Equal(0, this.session.Query<Post>().Count(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task SingleEntityInsertAsyncWorks()
        {
            await this.session.InsertAsync(new Post { Title = "Bar" });
            Assert.Equal(1, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
        }

        [Fact]
        public async Task ExtensionMultiEntityInsertAsyncWorks()
        {
            await this.session.InsertAsync(new Post { Title = "Bar" }, new Post { Title = "Bar" });
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
        }

        [Fact]
        public async Task ArrayEntityInsertAsyncWorks()
        {
            var posts = new[] { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            await this.session.InsertAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
        }

        [Fact]
        public async Task ListEntityInsertAsyncWorks()
        {
            var posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            await this.session.InsertAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
        }

        [Fact]
        public async Task ClassListPropertyEntityInsertAsyncWorks()
        {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } } };
            await this.session.InsertAsync(parent.Posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
        }

        [Fact]
        public async Task SingleEntitySaveAsyncWorks()
        {
            var entity = await this.session.GetAsync<Post>(1);
            entity.Title = "Bar";
            await this.session.SaveAsync(entity);
            Assert.Equal(1, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
            Assert.Empty(await this.session.Query<Post>().Where(p => p.Title == "Foo").ToArrayAsync());
        }

        [Fact]
        public async Task ExtensionMultiEntitySaveAsyncWorks()
        {
            var entity1 = this.session.Query<Post>().First();
            var entity2 = new Post { Title = "Foo" };
            await this.session.InsertAsync(entity2);
            entity1.Title = "Bar";
            entity2.Title = "Bar";
            await this.session.SaveAsync(entity1, entity2);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Bar"));
            Assert.Empty(await this.session.Query<Post>().Where(p => p.Title == "Foo").ToArrayAsync());
        }

        [Fact]
        public async Task ArrayEntitySaveAsyncWorks()
        {
            var posts = new[] { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            await this.session.InsertAsync(posts);
            foreach (var post in posts)
            {
                post.Title = "Banana";
            }

            await this.session.SaveAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task ListEntitySaveAsyncWorks()
        {
            var posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } };
            await this.session.InsertAsync(posts);
            foreach (var post in posts)
            {
                post.Title = "Banana";
            }

            await this.session.SaveAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task ClassListPropertyEntitySaveAsyncWorks()
        {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Bar" }, new Post { Title = "Bar" } } };
            await this.session.InsertAsync(parent.Posts);
            foreach (var post in parent.Posts)
            {
                post.Title = "Banana";
            }

            await this.session.SaveAsync(parent.Posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task SingleEntityDeleteAsyncWorks()
        {
            var entity = await this.session.GetAsync<Post>(1);
            await this.session.DeleteAsync(entity);
            Assert.Empty(await this.session.Query<Post>().ToArrayAsync());
        }

        [Fact]
        public async Task ExtensionMultiEntityDeleteAsyncWorks()
        {
            var entity1 = await this.session.Query<Post>().FirstAsync();
            var entity2 = new Post { Title = "Foo" };
            await this.session.InsertAsync(entity2);
            await this.session.DeleteAsync(entity1, entity2);
            Assert.Empty(await this.session.Query<Post>().ToArrayAsync());
        }

        [Fact]
        public async Task ArrayEntityDeleteAsyncWorks()
        {
            var posts = new[] { new Post { Title = "Banana" }, new Post { Title = "Banana" } };
            await this.session.InsertAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
            await this.session.DeleteAsync(posts);
            Assert.Equal(0, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task ListEntityDeleteAsyncWorks()
        {
            var posts = new List<Post> { new Post { Title = "Banana" }, new Post { Title = "Banana" } };
            await this.session.InsertAsync(posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
            await this.session.DeleteAsync(posts);
            Assert.Equal(0, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        [Fact]
        public async Task ClassListPropertyEntityDeleteAsyncWorks()
        {
            var parent = new PostParent { Posts = new List<Post> { new Post { Title = "Banana" }, new Post { Title = "Banana" } } };
            await this.session.InsertAsync(parent.Posts);
            Assert.Equal(2, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
            await this.session.DeleteAsync(parent.Posts);
            Assert.Equal(0, await this.session.Query<Post>().CountAsync(p => p.Title == "Banana"));
        }

        public void Dispose() {
            this.session?.Dispose();
        }

        class PostParent {
            public IList<Post> Posts { get; set; }
        }
    }
}