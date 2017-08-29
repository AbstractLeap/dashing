namespace Dashing.Tools.Tests.Migration {
    using Dashing.Configuration;
    using Dashing.Migration;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class TableNameEqualityComparerTests {
        [Fact]
        public void IsCaseInsensitive() {
            var map1 = new Map<Post>() { Table = "posts" };
            var map2 = new Map<Post>() { Table = "Posts" };
            var comparer = new TableNameEqualityComparer();
            Assert.True(comparer.Equals(map1, map2));
        }
    }
}