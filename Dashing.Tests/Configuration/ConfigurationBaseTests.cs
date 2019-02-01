namespace Dashing.Tests.Configuration {
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class ConfigurationBaseTests {

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
        public void ConstructorThrowsOnNullMapper() {
            Assert.Throws<ArgumentNullException>(
                () =>
                new CustomConfiguration(null));
        }

        [Fact]
        public void ManyToOneDbTypeSetCorrectly() {
            var config = new CustomConfigurationWithIndividualAdds(new DefaultMapper(new DefaultConvention()));
            Assert.Equal(DbType.Int32, config.GetMap<Post>().Columns["Author"].DbType);
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
            Assert.Equal(18, target.Maps.Count());
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Blog)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Comment)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Like)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Post)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(PostTag)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(Tag)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(User)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(BoolClass)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(ThingWithNullable)));
            Assert.Equal(1, target.Maps.Count(m => m.Type == typeof(ReferencesThingWithNullable)));
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
            Assert.Throws<ArgumentException>(() => { target.GetMap(typeof(Blog)); });
        }
        
        private static Mock<IMapper> MakeMockMapper() {
            return new Mock<IMapper>(MockBehavior.Strict);
        }

        private static Mock<IMapper> SetupAllMaps() {
            var mockMapper = SetupPostAndUserMaps();
            mockMapper.Setup(m => m.MapFor(typeof(Blog), It.IsAny<IConfiguration>())).Returns(new Map<Blog>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Comment), It.IsAny<IConfiguration>())).Returns(new Map<Comment>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Tag), It.IsAny<IConfiguration>())).Returns(new Map<Tag>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(PostTag), It.IsAny<IConfiguration>())).Returns(new Map<PostTag>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Like), It.IsAny<IConfiguration>())).Returns(new Map<Like>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(BoolClass), It.IsAny<IConfiguration>())).Returns(new Map<BoolClass>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(ThingWithNullable), It.IsAny<IConfiguration>())).Returns(new Map<ThingWithNullable>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(ReferencesThingWithNullable), It.IsAny<IConfiguration>())).Returns(new Map<ReferencesThingWithNullable>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(OneToOneLeft), It.IsAny<IConfiguration>())).Returns(new Map<OneToOneLeft>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(OneToOneRight), It.IsAny<IConfiguration>())).Returns(new Map<OneToOneRight>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Pair), It.IsAny<IConfiguration>())).Returns(new Map<Pair>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Post), It.IsAny<IConfiguration>())).Returns(new Map<Post>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(User), It.IsAny<IConfiguration>())).Returns(new Map<User>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(LineItem), It.IsAny<IConfiguration>())).Returns(new Map<LineItem>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Order), It.IsAny<IConfiguration>())).Returns(new Map<Order>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Customer), It.IsAny<IConfiguration>())).Returns(new Map<Customer>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(Delivery), It.IsAny<IConfiguration>())).Returns(new Map<Delivery>()).Verifiable();
            mockMapper.Setup(m => m.MapFor(typeof(ThingThatReferencesOrderNullable), It.IsAny<IConfiguration>())).Returns(new Map<ThingThatReferencesOrderNullable>()).Verifiable();
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

        private class CustomConfiguration : BaseConfiguration {
            public CustomConfiguration(
                IMapper mapper)
                : base(mapper) {
            }
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

        private class BasicConfiguration : CustomConfiguration {
            public BasicConfiguration()
                : base(new DefaultMapper(new DefaultConvention())) {
                this.Add<Post>();
            }
        }

        private class BasicConfigurationWithCodeManager : BaseConfiguration {
            public BasicConfigurationWithCodeManager() {
                this.Add<Post>();
            }
        }
    }
}