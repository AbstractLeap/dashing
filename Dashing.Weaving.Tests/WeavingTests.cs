namespace Dashing.Weaving.Tests {
    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    public class WeavingTests {
        [Fact]
        public void NullCollectionGetsInstantiatedInConstructor() {
            var foo = new Foo();
            Assert.NotNull(foo.Bars);
        }

        [Fact]
        public void AlreadyInstantiatedCollectionDoesNotGetInstantiatedInConstructor() {
            var bar = new Bar();
            Assert.NotEmpty(bar.Ducks);
        }

        [Fact]
        public void NonAutoOneToManyGetsInstantiated() {
            var whopper = new Whopper();
            Assert.NotNull(whopper.Ducks);
            Assert.Equal("Burger", whopper.GetFilling());
        }
    }
}