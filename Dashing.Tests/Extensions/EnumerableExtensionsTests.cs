namespace Dashing.Tests.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class EnumerableExtensionsTests {
        [Fact]
        public void TopologicalSortWorks() {
            var maps = new CustomConfig().Maps;
            var sortedMaps = maps.OrderTopologically();
            var mapIndexes = new Dictionary<Type, int>();
            int i = 0;
            foreach (var map in sortedMaps) {
                mapIndexes.Add(map.Type, i++);
            }

            Assert.True(mapIndexes[typeof(Post)] < mapIndexes[typeof(Blog)]);
            Assert.True(mapIndexes[typeof(Comment)] < mapIndexes[typeof(Post)]);
            Assert.True(mapIndexes[typeof(PostTag)] < mapIndexes[typeof(Tag)]);
            Assert.True(mapIndexes[typeof(Like)] < mapIndexes[typeof(Comment)]);
            Assert.True(mapIndexes[typeof(PostTag)] < mapIndexes[typeof(Post)]);
        }

        [Fact]
        public void TopologicalSortWorksWithoutOneToMany() {
            var maps = new CustomConfig().Maps;
            var blogMap = maps.First(m => m.Type == typeof(Blog));
            blogMap.Columns.Remove("Posts");
            var sortedMaps = maps.OrderTopologically();
            var mapIndexes = new Dictionary<Type, int>();
            int i = 0;
            foreach (var map in sortedMaps) {
                mapIndexes.Add(map.Type, i++);
            }

            Assert.True(mapIndexes[typeof(Post)] < mapIndexes[typeof(Blog)]);
            Assert.True(mapIndexes[typeof(Comment)] < mapIndexes[typeof(Post)]);
            Assert.True(mapIndexes[typeof(PostTag)] < mapIndexes[typeof(Tag)]);
            Assert.True(mapIndexes[typeof(Like)] < mapIndexes[typeof(Comment)]);
            Assert.True(mapIndexes[typeof(PostTag)] < mapIndexes[typeof(Post)]);
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                    this.AddNamespaceOf<Post>();
            }
        }
    }
}
