namespace Dashing.ElasticSearch.Tests {
    using Xunit;

    public class ListenerTests {
        [Fact(Skip = "Calls real ElasticSearch instance")]
        public void AddEntityIndexes() {
            //var clientFactory = new ElasticClientFactory("http://helga-ubuntu.cloudapp.net", "test", "superuser", "Fjj4axy3lou5nab5");
            //var listener = new ElasticSearchEventListener(clientFactory);
            //listener.AddTypeToIndex(typeof(Post));
            //var session = new Mock<ISession>();
            //session.Setup(s => s.Configuration).Returns(new MockConfiguration().AddNamespaceOf<Post>());
            //listener.OnPostInsert(new Post { PostId = 1, Content = "This is some great content!", Title = "This is the title" }, session.Object);
            //var post = clientFactory.Create().Get<Post>(s => s.Id("1").Type("Post"));
            //Assert.True(post.Found);
        }
    }
}