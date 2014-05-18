namespace TopHat.Tests.Configuration {
  using System;
  using System.Data;
  using System.Linq;

  using Moq;

  using global::TopHat.Configuration;

  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class ConfigurationBaseTests {
    private const string DummyConnectionString = "Host=dummy.local";

    private const string ExampleTableName = "foo";

    private static readonly Mock<IEngine> MockEngine = new Mock<IEngine>(MockBehavior.Strict);

    private static readonly Mock<IMapper> MockMapper = new Mock<IMapper>(MockBehavior.Strict);

    private static readonly Mock<ISessionFactory> MockSessionFactory = new Mock<ISessionFactory>(MockBehavior.Strict);

    [Fact]
    public void EmptyConfigurationReturnsEmptyMaps() {
      var target = new CustomConfiguration();
      Assert.Empty(target.Maps);
    }

    [Fact]
    public void NonEmptyConfigurationReturnsNonEmptyMaps() {
      var target = new CustomConfigurationWithIndividualAdds();
      Assert.NotEmpty(target.Maps);
    }

    [Fact]
    public void ConstructorThrowsOnNullEngine() {
      Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(null, DummyConnectionString, MockMapper.Object, MockSessionFactory.Object));
    }

    [Fact]
    public void ConstructorThrowsOnNullConnectionString() {
      Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MockEngine.Object, null, MockMapper.Object, MockSessionFactory.Object));
    }

    [Fact]
    public void ConstructorThrowsOnNullMapper() {
      Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MockEngine.Object, DummyConnectionString, null, MockSessionFactory.Object));
    }

    [Fact]
    public void ConstructorThrowsOnNullSessionFactory() {
      Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MockEngine.Object, DummyConnectionString, MockMapper.Object, null));
    }

    [Fact]
    public void BeginSessionCreatesConnectionAndDelegatesToSessionFactory() {
      var connection = new Mock<IDbConnection>();
      var session = new Mock<ISession>();
      MockEngine.Setup(m => m.Open(DummyConnectionString)).Returns(connection.Object).Verifiable();
      MockSessionFactory.Setup(m => m.Create(MockEngine.Object, connection.Object)).Returns(session.Object).Verifiable();

      var target = new CustomConfiguration();
      var actual = target.BeginSession();

      Assert.Equal(session.Object, actual);
      MockEngine.Verify();
      MockSessionFactory.Verify();
    }

    [Fact]
    public void BeginSessionWithConnectionDelegatesToSessionFactory() {
      var connection = new Mock<IDbConnection>();
      var session = new Mock<ISession>();
      MockSessionFactory.Setup(m => m.Create(MockEngine.Object, connection.Object)).Returns(session.Object).Verifiable();

      var target = new CustomConfiguration();
      var actual = target.BeginSession(connection.Object);

      Assert.Equal(session.Object, actual);
      MockSessionFactory.Verify();
    }

    [Fact]
    public void BeginSessionWithConnectionAndTransactionDelegatesToSessionFactory() {
      var connection = new Mock<IDbConnection>();
      var transaction = new Mock<IDbTransaction>();
      var session = new Mock<ISession>();
      MockSessionFactory.Setup(m => m.Create(MockEngine.Object, connection.Object, transaction.Object)).Returns(session.Object).Verifiable();

      var target = new CustomConfiguration();
      var actual = target.BeginSession(connection.Object, transaction.Object);

      Assert.Equal(session.Object, actual);
      MockSessionFactory.Verify();
    }

    [Fact]
    public void AddEntitiesByGenericAreMapped() {
      SetupPostAndUserMaps();

      var target = new CustomConfigurationWithIndividualAdds();

      Assert.NotNull(target);
      Assert.Equal(2, target.Maps.Count());
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
      MockMapper.Verify();
    }

    [Fact]
    public void AddEntitiesByTypeAreMapped() {
      SetupPostAndUserMaps();

      var target = new CustomConfigurationWithAddEnumerable();

      Assert.NotNull(target);
      Assert.Equal(2, target.Maps.Count());
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
      MockMapper.Verify();
    }

    [Fact]
    public void AddEntiesInNamespaceAreMapped() {
      MockMapper.Setup(m => m.MapFor<Blog>()).Returns(default(Map<Blog>)).Verifiable();
      MockMapper.Setup(m => m.MapFor<Comment>()).Returns(default(Map<Comment>)).Verifiable();
      SetupPostAndUserMaps();

      var target = new CustomConfigurationWithAddNamespace();

      Assert.NotNull(target);
      Assert.Equal(4, target.Maps.Count());
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Blog)));
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Comment)));
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
      Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
      MockMapper.Verify();
    }

    private static void SetupPostAndUserMaps() {
      MockMapper.Setup(m => m.MapFor<Post>()).Returns(default(Map<Post>)).Verifiable();
      MockMapper.Setup(m => m.MapFor<User>()).Returns(default(Map<User>)).Verifiable();
    }

    [Fact]
    public void SetupEntityCreatesAndConfiguresMap() {
      var target = new CustomConfigurationWithSetup();
      var actual = target.Maps.Single(m => m.Type == typeof(User));
      Assert.Equal(ExampleTableName, actual.Table);
    }

    [Fact]
    public void AddEntityAndSetupConfiguresMap() {
      var target = new CustomConfigurationWithAddAndSetup();
      var actual = target.Maps.Single(m => m.Type == typeof(User));
      Assert.Equal(ExampleTableName, actual.Table);
    }

    private class CustomConfiguration : ConfigurationBase {
      public CustomConfiguration(IEngine engine, string connectionString, IMapper mapper, ISessionFactory sessionFactory)
        : base(engine, connectionString, mapper, sessionFactory) { }

      public CustomConfiguration()
        : base(MockEngine.Object, ConfigurationBaseTests.DummyConnectionString, MockMapper.Object, MockSessionFactory.Object) { }
    }

    private class CustomConfigurationWithIndividualAdds : CustomConfiguration {
      public CustomConfigurationWithIndividualAdds() {
        this.Add<Post>();
        this.Add<User>();
      }
    }

    private class CustomConfigurationWithAddEnumerable : CustomConfiguration {
      public CustomConfigurationWithAddEnumerable() {
        this.Add(new[] { typeof(Post), typeof(User) });
      }
    }

    private class CustomConfigurationWithAddNamespace : CustomConfiguration {
      public CustomConfigurationWithAddNamespace() {
        this.AddNamespaceOf<Post>();
      }
    }

    private class CustomConfigurationWithAddAndSetup : CustomConfiguration {
      public CustomConfigurationWithAddAndSetup() {
        this.AddNamespaceOf<Post>();
        this.Setup<User>().Table = ConfigurationBaseTests.ExampleTableName;
      }
    }

    private class CustomConfigurationWithSetup : CustomConfiguration {
      public CustomConfigurationWithSetup() {
        this.Setup<User>().Table = ConfigurationBaseTests.ExampleTableName;
      }
    }
  }
}