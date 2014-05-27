namespace TopHat.Tests.Configuration {
    using TopHat.Configuration;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class MapExtensionsTests {
        private const string ExampleString = "foo";

        private const string Username = "Username";

        private const string UserId = "UserId";

        [Fact]
        public void TableIsSet() {
            var map = this.MakeMap();
            map.Table(ExampleString);
            Assert.Equal(ExampleString, map.Table);
        }

        [Fact]
        public void SchemaIsSet() {
            var map = this.MakeMap();
            map.Schema(ExampleString);
            Assert.Equal(ExampleString, map.Schema);
        }

        [Fact]
        public void PrimaryKeyIsSet() {
            var map = this.MakeMap();
            map.PrimaryKey(p => p.UserId);
            Assert.Equal(UserId, map.PrimaryKey.Name);
            Assert.True(map.PrimaryKey.IsPrimaryKey);
        }

        [Fact]
        public void PrimaryKeyUnsetsFlagOnOtherColumns() {
            var map = this.MakeMap();
            map.Property(m => m.UserId).IsPrimaryKey = true;

            map.PrimaryKey(p => p.Username);

            Assert.False(map.Property(m => m.UserId).IsPrimaryKey);
            Assert.True(map.Property(m => m.Username).IsPrimaryKey);
        }

        [Fact]
        public void PropertyReturnsColumn() {
            var map = this.MakeMap();
            var property = map.Property(u => u.Username);
            Assert.NotNull(property);
            Assert.Equal(Username, property.Name);
        }

        private Map<User> MakeMap() {
            var map = new Map<User>();
            map.Columns.Add(UserId, new Column<int> { Name = UserId });
            map.Columns.Add(Username, new Column<string> { Name = Username });
            return map;
        }

        /*
    [Fact]
    public void SpecifySingleKey() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<User>().Key(u => u.Username);
      Assert.Equal("Username", config.Maps[typeof(Post)].PrimaryKey);
    }

    [Fact]
    public void PKDbGenerated() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<User>().PrimaryKeyDatabaseGenerated(true);
      Assert.True(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
    }

    [Fact]
    public void PKDbGeneratedFalse() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<User>().PrimaryKeyDatabaseGenerated(false);
      Assert.False(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
    }

    [Fact]
    public void SpecifySchema() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<User>().Schema("security");
      Assert.Equal("security", config.Maps[typeof(User)].Schema);
    }

    [Fact]
    public void SpecifyTable() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<User>().Table("Identities");
      Assert.Equal("Identities", config.Maps[typeof(User)].Table);
    }

    [Fact]
    public void DefaultConfigurationPKDbGenerated() {
      var config = new DefaultConfiguration().Configure().Add<Post>();
      Assert.False(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
    }

    [Fact]
    public void DefaultConfigurationFKIndexesGenerated() {
      var config = new DefaultConfiguration().Configure();
      Assert.True(config.Convention.GenerateIndexesOnForeignKeysByDefault);
    }

    [Fact]
    public void IndexSetCorrectlySingleProperty() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<Post>().Index(p => p.Title);
      Assert.True(config.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.First() == "Title") == 1);
    }

    [Fact]
    public void IndexSetCorrectlyMultipleProperties() {
      var config = new DefaultConfiguration().Configure();
      config.Setup<Post>().Index(p => new { p.PostId, p.Title });
      Assert.True(config.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.Contains("Title") && l.Contains("PostId")) == 1);
    }*/
    }
}