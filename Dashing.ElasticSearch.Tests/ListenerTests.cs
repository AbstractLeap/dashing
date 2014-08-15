using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashing.Tests;
using Dashing.Tests.TestDomain;
using Moq;
using Xunit;

namespace Dashing.ElasticSearch.Tests {
    public class ListenerTests {
        [Fact]
        public void AddEntityIndexes() {
            var clientFactory = new SingletonClientFactory("http://localhost:9200", "test");
            var listener = new ElasticSearchEventListener(clientFactory);
            listener.AddTypeToIndex(typeof(Post));
            var session = new Mock<ISession>();
            session.Setup(s => s.Configuration).Returns(new MockConfiguration().AddNamespaceOf<Post>());
            listener.OnPostInsert(new Post { PostId = 1, Content = "This is some great content!", Title = "This is the title" }, session.Object);
            var post = clientFactory.Create().Get<Post>(s => s.Id("1").Type("Post"));
            Assert.True(post.Found);
        }


    }
}
