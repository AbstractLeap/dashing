namespace TopHat.Tests.Configuration {
    using System;

    using TopHat.Configuration;
    using TopHat.Tests.Extensions;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class MapTests {
        [Fact]
        public void ColumnsCollectionIsInitialised() {
            var target = new Map<string>();
            Assert.NotNull(target.Columns);
        }

        [Fact]
        public void FromPopulatesAllProperties() {
            // assemble a populated column
            var imap = new Map<string>().Populate(new Column<string>() as IColumn) as IMap;

            // act
            var map = Map<string>.From(imap);

            // assert all properties are equal
            var columnType = imap.GetType();
            var genericColumnType = map.GetType();
            foreach (var prop in columnType.GetProperties()) {
                Assert.Equal(prop.GetValue(imap, null), genericColumnType.GetProperty(prop.Name).GetValue(map, null));
            }
        }

        [Fact]
        public void GetPrimaryKeyValueWorks() {
            var map = new Map<Post>();
            map.Columns.Add("PostId", new Column<int> { IsPrimaryKey = true, Map = map, Name = "PostId" });
            map.PrimaryKey = map.Columns["PostId"];
            var post = new Post { PostId = 123 };
            Assert.Equal(post.PostId, map.GetPrimaryKeyValue(post));
        }

        [Fact]
        public void SetPrimaryKeyValueWorks() {
            var map = new Map<Post>();
            map.Columns.Add("PostId", new Column<int> { IsPrimaryKey = true, Map = map, Name = "PostId" });
            map.PrimaryKey = map.Columns["PostId"];
            var post = new Post { PostId = 123 };
            map.SetPrimaryKeyValue(post, 256);
            Assert.Equal(256, post.PostId);
        }

        [Fact]
        public void UsePrimaryKeyGetterWithoutPrimaryKeyColumnThrow() {
            var map = new Map<Post>();
            map.Columns.Add("PostId", new Column<int> { IsPrimaryKey = true, Map = map, Name = "PostId" });
            var post = new Post { PostId = 123 };
            Assert.Throws<Exception>(() => map.GetPrimaryKeyValue(post));
        }

        [Fact]
        public void UsePrimaryKeySetterWithoutPrimaryKeyColumnThrow()
        {
            var map = new Map<Post>();
            map.Columns.Add("PostId", new Column<int> { IsPrimaryKey = true, Map = map, Name = "PostId" });
            var post = new Post { PostId = 123 };
            Assert.Throws<Exception>(() => map.SetPrimaryKeyValue(post, 123));
        }

        [Fact]
        public void NonGenericPrimaryKeyGetterWorks() {
            var map = new Map<Post>();
            map.Columns.Add("PostId", new Column<int> { IsPrimaryKey = true, Map = map, Name = "PostId" });
            map.PrimaryKey = map.Columns["PostId"];
            var post = new Post { PostId = 123 };
            Assert.Equal(post.PostId, map.GetPrimaryKeyValue((object)post));
        }
    }
}