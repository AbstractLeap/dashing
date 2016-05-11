namespace Dashing.Weaving.Tests {
    using System;
    using System.Linq;

    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    [Collection("Weaving Tests")]
    public class ForeignKeyTests {
        [Fact]
        public void NullReturnsNull() {
            var bar = new Bar();
            Assert.Null(bar.Foo);
        }

        [Fact]
        public void FkFieldExists() {
            Assert.True(typeof(Bar).GetFields().SingleOrDefault(f => f.Name == "FooId") != null);
        }

        [Fact]
        public void FkFieldHasCorrectType() {
            Assert.Equal(typeof(Nullable<>).MakeGenericType(typeof(int)), typeof(Bar).GetFields().Single(f => f.Name == "FooId").FieldType);
        }

        [Fact]
        public void ReturnFooWithId() {
            var bar = new Bar();
            typeof(Bar).GetField("FooId").SetValue(bar, 3);
            Assert.Equal(3, bar.Foo.FooId);
        }

        [Fact]
        public void ChangeFooChanges() {
            var bar = new Bar();
            typeof(Bar).GetField("FooId").SetValue(bar, 3);
            bar.Foo = new Foo { FooId = 4 };
            Assert.Equal(4, bar.Foo.FooId);
        }

        [Fact]
        public void ChangeToNullResets() {
            var bar = new Bar();
            typeof(Bar).GetField("FooId").SetValue(bar, 3);
            bar.Foo = null;
            Assert.Null(bar.Foo);
        }
    }
}