namespace Dashing.Weaving.Tests {
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;

    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    public class EqualityTests : IClassFixture<WeavingFixture> {
        [Fact]
        public void GetHashCodeReturnsIdFactor() {
            var bar = new Bar { Id = 3 };
            Assert.Equal(17 * 29 + 3, bar.GetHashCode());
        }

        [Fact]
        public void NoIdReturnsBaseHashCode() {
            var bar = new Bar();
            Assert.Equal(RuntimeHelpers.GetHashCode(bar), bar.GetHashCode());
        }

        [Fact]
        public void SettingIdReturnsPrevIfGetHashCodeAlreadyCalled() {
            var bar = new Bar();
            var hash = bar.GetHashCode();
            bar.Id = 3;
            Assert.Equal(hash, bar.GetHashCode());
        }

        [Fact]
        public void EqualsNullIsFalse() {
            var bar = new Bar();
            Assert.False(bar.Equals(null));
        }

        [Fact]
        public void EqualsNonBarIsFalse() {
            var bar = new Bar();
            Assert.False(bar.Equals(new Foo()));
        }

        [Fact]
        public void NonSamePrimaryKeyNotEqual() {
            var bar = new Bar { Id = 1 };
            var otherBar = new Bar { Id = 2 };
            Assert.False(bar.Equals(otherBar));
            Assert.False(otherBar.Equals(bar));
        }

        [Fact]
        public void SamePrimaryKeyEqual() {
            var bar = new Bar { Id = 2 };
            var otherBar = new Bar { Id = 2 };
            Assert.True(bar.Equals(otherBar));
            Assert.True(otherBar.Equals(bar));
        }

        [Fact]
        public void SameInstanceIsEqual() {
            var bar = new Bar { Id = 3 };
            Assert.True(bar.Equals(bar));
        }

        [Fact]
        public void GetHashCodeNotOverwrittenIfSupplied() {
            var thing = new IveGotMethods();
            Assert.Equal(42, thing.GetHashCode());
        }

        [Fact]
        public void GetEqualsNotOverriddenIfSupplied() {
            var thing = new IveGotMethods();
            Assert.True(thing.Equals(null));
        }
    }
}