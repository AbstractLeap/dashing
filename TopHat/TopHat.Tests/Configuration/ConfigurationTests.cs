using Moq;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.Configuration {
	public class ConfigurationBaseTests {
		private const string DummyConnectionString = "Host=dummy.local";
		private static readonly Mock<IMapper> MockMapper = new Mock<IMapper>(MockBehavior.Strict);
		private static readonly Mock<ISessionFactory> MockSessionFactory = new Mock<ISessionFactory>(MockBehavior.Strict);
		private static readonly Mock<IQueryFactory> MockQueryFactory = new Mock<IQueryFactory>(MockBehavior.Strict);

		[Fact]
		public void EmptyConfiguration_RendersEmptyMap() {
			var target = new CustomConfiguration();
			Assert.Empty(target.Maps);
		}

		private class CustomConfiguration : ConfigurationBase {
			public CustomConfiguration() : base(Engines.SqlServer, DummyConnectionString, MockMapper.Object, MockSessionFactory.Object, MockQueryFactory.Object) {}
		}

		[Fact]
		public void AddEntitiesByGeneric_AreMapped() {
			MockMapper.Setup(m => m.MapFor(typeof (Post)))
			          .Returns(default(Map))
			          .Verifiable();
			MockMapper.Setup(m => m.MapFor(typeof (User)))
			          .Returns(default(Map))
			          .Verifiable();

			var target = new CustomConfiguration1();

			MockMapper.Verify();
		}

		private class CustomConfiguration1 : CustomConfiguration {
			public CustomConfiguration1() {
				Add<Post>();
				Add<User>();
			}
		}

		[Fact]
		public void AddEntitiesByType_AreMapped() {
			MockMapper.Setup(m => m.MapFor(typeof (Post)))
			          .Returns(default(Map))
			          .Verifiable();
			MockMapper.Setup(m => m.MapFor(typeof (User)))
			          .Returns(default(Map))
			          .Verifiable();

			var target = new CustomConfiguration2();

			MockMapper.Verify();
		}

		private class CustomConfiguration2 : CustomConfiguration {
			public CustomConfiguration2() {
				Add(new[] {typeof (Post), typeof (User)});
			}
		}

		[Fact]
		public void AddEntiesInNamespace_AreMapped() {
			MockMapper.Setup(m => m.MapFor(typeof (Blog)))
			          .Returns(default(Map))
			          .Verifiable();
			MockMapper.Setup(m => m.MapFor(typeof (Comment)))
			          .Returns(default(Map))
			          .Verifiable();
			MockMapper.Setup(m => m.MapFor(typeof (Post)))
			          .Returns(default(Map))
			          .Verifiable();
			MockMapper.Setup(m => m.MapFor(typeof (User)))
			          .Returns(default(Map))
			          .Verifiable();

			var target = new CustomConfiguration3();

			MockMapper.Verify();
		}

		private class CustomConfiguration3 : CustomConfiguration {
			public CustomConfiguration3() {
				AddNamespaceOf<Post>();
				;
			}
		}
	}
}