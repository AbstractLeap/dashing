namespace Dashing.Tests.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.OneToOne;

    using Xunit;

    public class EnumerableExtensionsTests {
        [Fact]
        public void TopologicalSortWorks() {
            var maps = new CustomConfig().Maps;
            var sortedMaps = maps.OrderTopologically().OrderedMaps;
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
            var sortedMaps = maps.OrderTopologically().OrderedMaps;
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
        public void SubsetWorks() {
            var list = new[] { 1, 2, 3 };
            var subsets = list.Subsets();
            Assert.Equal(8, subsets.Count());
        }

        [Fact]
        public void OneToOneGetsSpotted() {
            var maps = new CustomOneToOneConfig().Maps;
            var sortedMaps = maps.OrderTopologically();
            Assert.Equal(
                new[] { maps.First(m => m.Type == typeof(OneToOneLeft)), maps.First(m => m.Type == typeof(OneToOneRight)) },
                sortedMaps.OneToOneMaps);
        }

        [Fact]
        public void SelfReferencingGetsSpotted() {
            var maps = new CustomOneToOneConfig().Maps;
            var sortedMaps = maps.OrderTopologically();
            Assert.Equal(
                new[] { maps.First(m => m.Type == typeof(Category)) },
                sortedMaps.SelfReferencingMaps);
        }

        [Fact]
        public void OrderedMapsContainsSelfReferencing() {
            var maps = new CustomOneToOneConfig().Maps;
            var sortedMaps = maps.OrderTopologically();
            Assert.Contains(maps.First(m => m.Type == typeof(Category)),
                sortedMaps.OrderedMaps);
        }

        [Fact]
        public void OrderedMapsContainOneToOne() {
            var maps = new CustomOneToOneConfig().Maps;
            var sortedMaps = maps.OrderTopologically();
            Assert.Equal(2, 
                new[] { maps.First(m => m.Type == typeof(OneToOneLeft)), maps.First(m => m.Type == typeof(OneToOneRight)) }.Intersect(
                sortedMaps.OrderedMaps).Count());
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                    this.AddNamespaceOf<Post>();
            }
        }

        private class CustomOneToOneConfig : DefaultConfiguration {
            public CustomOneToOneConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
                this.AddNamespaceOf<OneToOneLeft>();
            }
        }
    }
}
