using System.Linq;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.Configuration {
	public class DefaultConfigurationTests {
		private const string ConnectionString = "Host=dummy.local";

		[Fact]
		public void EmptyConfiguration_RendersEmptyMap() {
			var config = 
				TopHat.Configure(Engines.SqlServer, ConnectionString);

			Assert.Empty(config.Maps);
		}

		[Fact]
		public void AddEntitiesByGeneric_AreMapped() {
			var config =
				TopHat.Configure(Engines.SqlServer, ConnectionString)
				      .Add<Post>()
				      .Add<User>();

			Assert.Equal(2, config.Maps.Count());
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (Post)));
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (User)));
		}

		[Fact]
		public void AddEntitiesByType_AreMapped() {
			var config =
				TopHat.Configure(Engines.SqlServer, ConnectionString)
				      .Add(new[] {typeof (Post), typeof (User)});

			Assert.Equal(2, config.Maps.Count());
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (Post)));
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (User)));
		}

		[Fact]
		public void AddEntiesInNamespace_AreMapped() {
			var config =
				TopHat.Configure(Engines.SqlServer, ConnectionString)
				      .AddNamespaceOf<Post>();

			Assert.Equal(4, config.Maps.Count());
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (Blog)));
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (Comment)));
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (Post)));
			Assert.Equal(1, config.Maps.Count(m => m.Type == typeof (User)));
		}
	}
}