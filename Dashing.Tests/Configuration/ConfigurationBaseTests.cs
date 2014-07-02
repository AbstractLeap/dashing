namespace Dashing.Tests.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Moq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class ConfigurationBaseTests {
        private const string DummyConnectionString = "Host=dummy.local";

        private const string ExampleTableName = "foo";

        [Fact]
        public void EmptyConfigurationReturnsEmptyMaps() {
            var target = new CustomConfiguration(MakeMockMapper().Object);
            Assert.Empty(target.Maps);
        }

        [Fact]
        public void NonEmptyConfigurationReturnsNonEmptyMaps() {
            var target = new CustomConfigurationWithIndividualAdds(SetupGenericMaps().Object);
            Assert.NotEmpty(target.Maps);
        }

        [Fact]
        public void ConstructorThrowsOnNullEngine() {
            Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(null, DummyConnectionString, MakeMockMapper().Object, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullConnectionString() {
            Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MakeMockEngine().Object, null, MakeMockMapper().Object, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullMapper() {
            Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MakeMockEngine().Object, DummyConnectionString, null, MakeMockSf().Object));
        }

        [Fact]
        public void ConstructorThrowsOnNullSessionFactory() {
            Assert.Throws<ArgumentNullException>(() => new CustomConfiguration(MakeMockEngine().Object, DummyConnectionString, MakeMockMapper().Object, null));
        }

        [Fact]
        public void BeginSessionCreatesConnectionAndDelegatesToSessionFactory() {
            // assemble
            var connection = new Mock<IDbConnection>();
            var session = new Mock<ISession>();

            var mockEngine = MakeMockEngine();
            mockEngine.Setup(m => m.Open(DummyConnectionString)).Returns(connection.Object).Verifiable();
            mockEngine.Setup(m => m.UseMaps(It.IsAny<Dictionary<Type, IMap>>()));

            var mockSessionFactory = MakeMockSf();

            var target = new CustomConfiguration(mockEngine.Object, MakeMockMapper().Object, mockSessionFactory.Object);
            mockSessionFactory.Setup(m => m.Create(connection.Object, target)).Returns(session.Object).Verifiable();

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
            mockEngine.Setup(m => m.UseMaps(It.IsAny<Dictionary<Type, IMap>>()));

            var mockSessionFactory = MakeMockSf();

            var target = new CustomConfiguration(mockEngine.Object, MakeMockMapper().Object, mockSessionFactory.Object);
            mockSessionFactory.Setup(m => m.Create(connection.Object, target)).Returns(session.Object).Verifiable();

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
            mockEngine.Setup(m => m.UseMaps(It.IsAny<Dictionary<Type, IMap>>()));

            var mockSessionFactory = MakeMockSf();

            var target = new CustomConfiguration(mockEngine.Object, MakeMockMapper().Object, mockSessionFactory.Object);
            mockSessionFactory.Setup(m => m.Create(connection.Object, transaction.Object, target)).Returns(session.Object).Verifiable();

            // act
            var actual = target.BeginSession(connection.Object, transaction.Object);

            // assert
            Assert.Equal(session.Object, actual);
            mockSessionFactory.Verify();
        }

        [Fact]
        public void AddEntitiesByGenericAreMapped() {
            var target = new CustomConfigurationWithIndividualAdds(SetupGenericMaps().Object);

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
            Assert.Equal(4, target.Maps.Count());
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Blog)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Comment)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
            mockMapper.Verify();
        }

        [Fact]
        public void SetupEntityCreatesAndConfiguresMap() {
            var target = new CustomConfigurationWithSetup(SetupGenericUserMap().Object);
            var actual = target.Maps.Single(m => m.Type == typeof(User));
            Assert.Equal(ExampleTableName, actual.Table);
        }

        [Fact]
        public void AddEntityAndSetupConfiguresMap() {
            var target = new CustomConfigurationWithAddAndSetup(SetupAllMaps().Object);
            var actual = target.Maps.Single(m => m.Type == typeof(User));
            Assert.Equal(ExampleTableName, actual.Table);
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

        private static Mock<IMapper> SetupAllMaps() {
            var mockMapper = SetupPostAndUserMaps();
            mockMapper.Setup(m => m.MapFor(typeof(Blog))).Returns(new Map<Blog>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Comment))).Returns(new Map<Comment>()).Verifiable();
            return mockMapper;
        }

        private static Mock<IMapper> SetupPostAndUserMaps() {
            var mock = SetupUserMap();
            mock.Setup(m => m.MapFor(typeof(Post))).Returns(new Map<Post>()).Verifiable();
            return mock;
        }

        private static Mock<IMapper> SetupUserMap() {
            var mock = new Mock<IMapper>(MockBehavior.Strict);
            mock.Setup(m => m.MapFor(typeof(User))).Returns(new Map<User>()).Verifiable();
            return mock;
        }

        private static Mock<IMapper> SetupGenericMaps() {
            var mock = SetupGenericUserMap();
            mock.Setup(m => m.MapFor<Post>()).Returns(new Map<Post>()).Verifiable();
            return mock;
        }

        private static Mock<IMapper> SetupGenericUserMap() {
            var mock = new Mock<IMapper>(MockBehavior.Strict);
            mock.Setup(m => m.MapFor<User>()).Returns(new Map<User>()).Verifiable();
            return mock;
        }

        private static Mock<ICodeGenerator> SetupCodeGenerator() {
            var mock = new Mock<ICodeGenerator>(MockBehavior.Strict);
            var mock2 = new Mock<IGeneratedCodeManager>(MockBehavior.Strict);
            mock.Setup(m => m.Generate(It.IsAny<IConfiguration>())).Returns(mock2.Object);
            return mock;
        }

        private class CustomConfiguration : ConfigurationBase {
            public CustomConfiguration(IEngine engine, string connectionString, IMapper mapper, ISessionFactory sessionFactory)
                : base(engine, connectionString, mapper, sessionFactory, SetupCodeGenerator().Object) { }
            
            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfiguration(IEngine engine, IMapper mapper, ISessionFactory sessionFactory)
                : this(engine, DummyConnectionString, mapper, sessionFactory) { }

            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfiguration(IMapper mapper)
                : base(MakeMockEngine().Object, DummyConnectionString, mapper, MakeMockSf().Object, SetupCodeGenerator().Object) { }
        }

        private class CustomConfigurationWithIndividualAdds : CustomConfiguration {
            public CustomConfigurationWithIndividualAdds(IMapper mapper)
                : base(mapper) {
                this.Add<Post>();
                this.Add<User>();
            }
        }

        private class CustomConfigurationWithAddEnumerable : CustomConfiguration {
            public CustomConfigurationWithAddEnumerable(IMapper mapper)
                : base(mapper) {
                this.Add(new[] { typeof(Post), typeof(User) });
            }
        }

        private class CustomConfigurationWithAddNamespace : CustomConfiguration {
            public CustomConfigurationWithAddNamespace(IMapper mapper)
                : base(mapper) {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigurationWithAddAndSetup : CustomConfiguration {
            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfigurationWithAddAndSetup(IMapper mapper)
                : base(mapper) {
                this.AddNamespaceOf<Post>();
                this.Setup<User>().Table = ExampleTableName;
            }
        }

        private class CustomConfigurationWithSetup : CustomConfiguration {
            [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "R# and StyleCop fight over this")]
            public CustomConfigurationWithSetup(IMapper mapper)
                : base(mapper) {
                this.Setup<User>().Table = ExampleTableName;
            }
        }
    }
}