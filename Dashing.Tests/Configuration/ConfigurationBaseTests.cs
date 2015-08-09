namespace Dashing.Tests.Configuration {
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class ConfigurationBaseTests{
        private static readonly ConnectionStringSettings DummyConnectionString = new ConnectionStringSettings {
            ConnectionString = "Data Source=dummy.local",
            ProviderName = "System.Data.SqlClient"
        };

        private const string ExampleTableName = "foo";

        [Fact]
        public void EmptyConfigurationReturnsEmptyMaps() {
            var target = new CustomConfiguration(MakeMockMapper().Object);
            Assert.Empty(target.Maps);
        }

        [Fact]
        public void NonEmptyConfigurationReturnsNonEmptyMaps() {
            var target = new CustomConfigurationWithIndividualAdds(SetupAllMaps().Object);
            Assert.NotEmpty(target.Maps);
        }

        [Fact]
        public void ConstructorThrowsOnNullEngine() {
            Assert.Throws<ArgumentNullException>(
                () => new CustomConfiguration(null, DummyConnectionString, MakeMockDbProviderFactory().Object, MakeMockMapper().Object, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullConnectionString() {
            Assert.Throws<ArgumentNullException>(
                () => new CustomConfiguration(MakeMockEngine().Object, null, MakeMockDbProviderFactory().Object, MakeMockMapper().Object, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullDbProviderFactory() {
            Assert.Throws<ArgumentNullException>(
                () => new CustomConfiguration(MakeMockEngine().Object, DummyConnectionString, null, MakeMockMapper().Object, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullMapper() {
            Assert.Throws<ArgumentNullException>(
                () => new CustomConfiguration(MakeMockEngine().Object, DummyConnectionString, MakeMockDbProviderFactory().Object, null, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullSessionFactory() {
            Assert.Throws<ArgumentNullException>(
                () => new CustomConfiguration(MakeMockEngine().Object, DummyConnectionString, MakeMockDbProviderFactory().Object, MakeMockMapper().Object, null));
        }

        [Fact]
        public void ManyToOneDbTypeSetCorrectly() {
            var config = new CustomConfigurationWithIndividualAdds(new DefaultMapper(new DefaultConvention()));
            Assert.Equal(DbType.Int32, config.GetMap<Post>().Columns["Author"].DbType);
        }

        [Fact]
        public void BeginSessionCreatesConnectionAndDelegatesToSessionFactory() {
            // assemble
            var mockEngine = MakeMockEngine();

            var mockProvider = MakeMockDbProviderFactory();
            var connection = new Mock<DbConnection>();
            mockProvider.Setup(m => m.CreateConnection()).Returns(connection.Object);

            var mockSessionFactory = MakeMockSf();
            var session = new Mock<ISession>();
            mockSessionFactory.Setup(m => m.Create(mockEngine.Object, connection.Object, null, true, false, false)).Returns(session.Object).Verifiable();

            var target = new CustomConfiguration(mockEngine.Object, mockProvider.Object, MakeMockMapper().Object, mockSessionFactory.Object);

            // act
            var actual = target.BeginSession();

            // assert
            Assert.Equal(session.Object, actual);
            mockEngine.Verify();
            mockSessionFactory.Verify();
        }

        [Fact]
        public void BeginSessionWithConnectionDelegatesToSessionFactory() {
            // assemble
            var connection = new Mock<IDbConnection>();
            var session = new Mock<ISession>();

            var mockEngine = MakeMockEngine();

            var mockSessionFactory = MakeMockSf();
            mockSessionFactory.Setup(m => m.Create(mockEngine.Object, connection.Object, null, false, false, false)).Returns(session.Object).Verifiable();

            var target = new CustomConfiguration(mockEngine.Object, MakeMockDbProviderFactory().Object, MakeMockMapper().Object, mockSessionFactory.Object);

            // act
            var actual = target.BeginSession(connection.Object);

            // assert
            Assert.Equal(session.Object, actual);
            mockSessionFactory.Verify();
        }

        [Fact]
        public void BeginSessionWithConnectionAndTransactionDelegatesToSessionFactory() {
            // assemble
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var session = new Mock<ISession>();

            var mockEngine = MakeMockEngine();

            var mockSessionFactory = MakeMockSf();
            mockSessionFactory.Setup(m => m.Create(mockEngine.Object, connection.Object, transaction.Object, false, false, false))
                              .Returns(session.Object)
                              .Verifiable();

            var target = new CustomConfiguration(mockEngine.Object, MakeMockDbProviderFactory().Object, MakeMockMapper().Object, mockSessionFactory.Object);

            // act
            var actual = target.BeginSession(connection.Object, transaction.Object);

            // assert
            Assert.Equal(session.Object, actual);
            mockSessionFactory.Verify();
        }

        [Fact]
        public void AddEntitiesByGenericAreMapped() {
            var target = new CustomConfigurationWithIndividualAdds(SetupAllMaps().Object);

            Assert.NotNull(target);
            Assert.Equal(2, target.Maps.Count());
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
            new Mock<IMapper>(MockBehavior.Strict).Verify();
        }

        [Fact]
        public void AddEntitiesByTypeAreMapped() {
            var mockMapper = SetupPostAndUserMaps();
            var target = new CustomConfigurationWithAddEnumerable(mockMapper.Object);

            Assert.NotNull(target);
            Assert.Equal(2, target.Maps.Count());
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
            mockMapper.Verify();
        }

        [Fact]
        public void AddEntiesInNamespaceAreMapped() {
            var mockMapper = SetupAllMaps();
            var target = new CustomConfigurationWithAddNamespace(mockMapper.Object);

            Assert.NotNull(target);
            Assert.Equal(8, target.Maps.Count());
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Blog)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Comment)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Like)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(PostTag)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Tag)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(BoolClass)));
            mockMapper.Verify();
        }

        [Fact]
        public void SetupEntityCreatesAndConfiguresMap() {
            var target = new CustomConfigurationWithSetup(SetupUserMap().Object);
            var actual = target.Maps.Single(m => m.Type == typeof(User));
            Assert.Equal(ExampleTableName, actual.Table);
        }

        [Fact]
        public void AddEntityAndSetupConfiguresMap() {
            var target = new CustomConfigurationWithAddAndSetup(SetupAllMaps().Object);
            var actual = target.Maps.Single(m => m.Type == typeof(User));
            Assert.Equal(ExampleTableName, actual.Table);
        }

        [Fact]
        public void HasMapReturnsTrueForMappedEntity() {
            // assemble
            var target = new BasicConfiguration();

            // act
            var actual = target.HasMap(typeof(Post));

            // assert
            Assert.True(actual);
        }

        [Fact]
        public void HasMapReturnsFalseForUnmappedEntity() {
            // assemble
            var target = new BasicConfigurationWithCodeManager();

            // act
            var actual = target.HasMap(typeof(Blog));

            // assert
            Assert.False(actual);
        }

        [Fact]
        public void GetMapReturnsMapForMappedEntity() {
            // assemble
            var target = new BasicConfiguration();

            // act
            var actual = target.GetMap(typeof(Post));

            // assert
            Assert.NotNull(actual);
            Assert.Equal(typeof(Post), actual.Type);
        }

        [Fact]
        public void GetMapThrowsForUnmappedEntity() {
            // assemble
            var target = new BasicConfigurationWithCodeManager();

            // assert
            Assert.Throws(typeof(ArgumentException), () => { target.GetMap(typeof(Blog)); });
        }

        private static Mock<ISessionFactory> MakeMockSf() {
            return new Mock<ISessionFactory>(MockBehavior.Strict);
        }

        private static Mock<IMapper> MakeMockMapper() {
            return new Mock<IMapper>(MockBehavior.Strict);
        }

        private static Mock<IEngine> MakeMockEngine() {
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            engine.SetupProperty(p => p.Configuration);
            return engine;
        }

        private static Mock<DbProviderFactory> MakeMockDbProviderFactory() {
            return new Mock<DbProviderFactory>();
        }

        private static Mock<IMapper> SetupAllMaps() {
            var mockMapper = SetupPostAndUserMaps();
            mockMapper.Setup(m => m.MapFor(typeof(Blog), It.IsAny<IConfiguration>())).Returns(new Map<Blog>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Comment), It.IsAny<IConfiguration>())).Returns(new Map<Comment>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Tag), It.IsAny<IConfiguration>())).Returns(new Map<Tag>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(PostTag), It.IsAny<IConfiguration>())).Returns(new Map<PostTag>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Like), It.IsAny<IConfiguration>())).Returns(new Map<Like>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(BoolClass), It.IsAny<IConfiguration>())).Returns(new Map<BoolClass>()).Verifiable();
            return mockMapper;
        }

        private static Mock<IMapper> SetupPostAndUserMaps() {
            var mock = SetupUserMap();
            mock.Setup(m => m.MapFor(typeof(Post), It.IsAny<IConfiguration>())).Returns(new Map<Post>()).Verifiable();
            return mock;
        }

        private static Mock<IMapper> SetupUserMap() {
            var mock = new Mock<IMapper>(MockBehavior.Strict);
            mock.Setup(m => m.MapFor(typeof(User), It.IsAny<IConfiguration>())).Returns(new Map<User>()).Verifiable();
            return mock;
        }

        [DoNotWeave]
        private class CustomConfiguration : ConfigurationBase {
            public CustomConfiguration(
                IEngine engine, ConnectionStringSettings connectionString, DbProviderFactory dbProviderFactory, IMapper mapper, ISessionFactory sessionFactory)
                : base(engine, connectionString, dbProviderFactory, mapper, sessionFactory) {}

            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfiguration(IEngine engine, DbProviderFactory dbProviderFactory, IMapper mapper, ISessionFactory sessionFactory)
                : this(engine, DummyConnectionString, dbProviderFactory, mapper, sessionFactory) {}

            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfiguration(IMapper mapper)
                : base(
                    MakeMockEngine().Object,
                    DummyConnectionString,
                    MakeMockDbProviderFactory().Object,
                    mapper,
                    MakeMockSf().Object
                    ) {}
        }

        [DoNotWeave]
        private class CustomConfigurationWithIndividualAdds : CustomConfiguration {
            public CustomConfigurationWithIndividualAdds(IMapper mapper)
                : base(mapper) {
                this.Add<Post>();
                this.Add<User>();
            }
        }

        [DoNotWeave]
        private class CustomConfigurationWithAddEnumerable : CustomConfiguration {
            public CustomConfigurationWithAddEnumerable(IMapper mapper)
                : base(mapper) {
                this.Add(new[] { typeof(Post), typeof(User) });
            }
        }

        [DoNotWeave]
        private class CustomConfigurationWithAddNamespace : CustomConfiguration {
            public CustomConfigurationWithAddNamespace(IMapper mapper)
                : base(mapper) {
                this.AddNamespaceOf<Post>();
            }
        }

        [DoNotWeave]
        private class CustomConfigurationWithAddAndSetup : CustomConfiguration {
            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfigurationWithAddAndSetup(IMapper mapper)
                : base(mapper) {
                this.AddNamespaceOf<Post>();
                this.Setup<User>().Table = ExampleTableName;
            }
        }

        [DoNotWeave]
        private class CustomConfigurationWithSetup : CustomConfiguration {
            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfigurationWithSetup(IMapper mapper)
                : base(mapper) {
                this.Setup<User>().Table = ExampleTableName;
            }
        }

        private class BasicConfiguration : CustomConfiguration {
            public BasicConfiguration()
                : base(new DefaultMapper(new DefaultConvention())) {
                this.Add<Post>();
            }
        }

        private class BasicConfigurationWithCodeManager : ConfigurationBase {
            public BasicConfigurationWithCodeManager()
                // ReSharper disable once RedundantNameQualifier - StyleCope and R# can't decide who is right on this one
                : base(
                    MakeMockEngine().Object,
                    DummyConnectionString,
                    MakeMockDbProviderFactory().Object,
                    new DefaultMapper(new DefaultConvention()),
                    MakeMockSf().Object) {
                this.Add<Post>();
            }
        }
    }
}