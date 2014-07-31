using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dashing.Configuration;
using Dashing.Extensions;
using Dashing.Tests.TestDomain;
using Xunit;

namespace Dashing.Tests.Extensions {
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
            Assert.True(mapIndexes[typeof(Tag)] < mapIndexes[typeof(PostTag)]);
            Assert.True(mapIndexes[typeof(Like)] < mapIndexes[typeof(Comment)]);
            Assert.True(mapIndexes[typeof(PostTag)] < mapIndexes[typeof(Post)]);
        }

        class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", "", "System.Data.SqlClient")) {
                    this.AddNamespaceOf<Post>();
            }
        }
    }
}
