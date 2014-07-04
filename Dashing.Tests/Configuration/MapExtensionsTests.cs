namespace Dashing.Tests.Configuration {
    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;

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
    }
}