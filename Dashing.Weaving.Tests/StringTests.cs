namespace Dashing.Weaving.Tests {
    using System.Runtime.CompilerServices;

    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;
    using Xunit;

    public class StringTests {
        [Fact]
        public void GuidPrimaryKeyWorks() {
            var thing = new EntityWithStringPrimaryKey { Id = "123", Name = "Foo" };
            Assert.IsAssignableFrom(typeof(ITrackedEntity), thing);
        }

        [Fact]
        public void GetHashCodeReturnsIdFactor() {
            var bar = new EntityWithStringPrimaryKey { Id = "123" };
            Assert.Equal(bar.Id.GetHashCode(), bar.GetHashCode());
        }

        [Fact]
        public void NoIdReturnsBaseHashCode() {
            var bar = new EntityWithStringPrimaryKey();
            Assert.Equal(RuntimeHelpers.GetHashCode(bar), bar.GetHashCode());
        }

        [Fact]
        public void SettingIdReturnsPrevIfGetHashCodeAlreadyCalled() {
            var bar = new EntityWithStringPrimaryKey();
            var hash = bar.GetHashCode();
            bar.Id = "456";
            Assert.Equal(hash, bar.GetHashCode());
        }

        [Fact]
        public void EqualsNullIsFalse() {
            var bar = new EntityWithStringPrimaryKey();
            Assert.False(bar.Equals(null));
        }

        [Fact]
        public void EqualsNonBarIsFalse() {
            var bar = new EntityWithStringPrimaryKey();
            Assert.False(bar.Equals(new Foo()));
        }

        [Fact]
        public void NonSamePrimaryKeyNotEqual() {
            var bar = new EntityWithStringPrimaryKey { Id = "123" };
            var otherBar = new EntityWithStringPrimaryKey { Id = "456" };
            Assert.False(bar.Equals(otherBar));
            Assert.False(otherBar.Equals(bar));
        }

        [Fact]
        public void SamePrimaryKeyEqual() {
            var pk = "123";
            var bar = new EntityWithStringPrimaryKey { Id = pk };
            var otherBar = new EntityWithStringPrimaryKey { Id = pk };
            Assert.True(bar.Equals(otherBar));
            Assert.True(otherBar.Equals(bar));
        }

        [Fact]
        public void SameInstanceIsEqual() {
            var bar = new EntityWithStringPrimaryKey { Id = "123" };
            Assert.True(bar.Equals(bar));
        }
    }
}