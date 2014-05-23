namespace TopHat.Tests.Tracking {
  using System.Linq;

  using global::TopHat.Tests.TestDomain;
  using global::TopHat.Tracking;

  using LinFu.Proxy.Interfaces;

  using Xunit;

  public class ChangeTrackingTests {
    private readonly IBehaviourFactory changeTrackBehaviourFactory;

    public ChangeTrackingTests() {
      this.changeTrackBehaviourFactory = new ChangeTrackingBehaviourFactory();
    }

    [Fact]
    public void DirtyPropertyCorrectlyMarked() {
      var post = this.GetChangeTrackingEntity();

      post.Title = "new Title";

      Assert.True(this.GetChangeTrackingBehaviour(post).DirtyProperties.Contains("Title"));
    }

    [Fact]
    public void SetPropertySameValueNoDirty() {
      var post = this.GetChangeTrackingEntity(new Post { Title = "Title" });
      post.Title = "Title";

      Assert.Empty(this.GetChangeTrackingBehaviour(post).DirtyProperties);
    }

    [Fact]
    public void CollectionDirty() {
      var post = this.GetChangeTrackingEntity();
      var comment = new Comment { Post = post, Content = "boo" };
      post.Comments.Add(comment);

      Assert.Same(comment, this.GetChangeTrackingBehaviour(post).AddedEntities.First().Value.First());
    }

    private Post GetChangeTrackingEntity(Post post = null) {
      var proxyManager = new ProxyManager();
      proxyManager.Register(this.changeTrackBehaviourFactory);

      return proxyManager.ProxyFor(post ?? new Post());
    }

    private Proxy<T> GetProxy<T>(T entity) {
      return ((IProxy)entity).Interceptor as Proxy<T>;
    }

    private ChangeTrackingBehaviour GetChangeTrackingBehaviour<T>(T entity) {
      return this.GetProxy(entity).Behaviours[this.changeTrackBehaviourFactory.Name] as ChangeTrackingBehaviour;
    }
  }
}