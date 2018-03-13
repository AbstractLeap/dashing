namespace Dashing.Weaving.Tests {
    using System;
    using System.Runtime.CompilerServices;

    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    public class GuidTests {
        [Fact]
        public void GuidPrimaryKeyWorks() {
            var thing = new EntityWithGuidPrimaryKey { Id = Guid.NewGuid(), Name = "Foo" };
            Assert.IsAssignableFrom<ITrackedEntity>(thing);
        }

        [Fact]
        public void GetHashCodeReturnsIdFactor() {
            var bar = new EntityWithGuidPrimaryKey { Id = Guid.NewGuid() };
            Assert.Equal(bar.Id.GetHashCode(), bar.GetHashCode());
        }

        [Fact]
        public void NoIdReturnsBaseHashCode() {
            var bar = new EntityWithGuidPrimaryKey();
            Assert.Equal(RuntimeHelpers.GetHashCode(bar), bar.GetHashCode());
        }

        [Fact]
        public void SettingIdReturnsPrevIfGetHashCodeAlreadyCalled() {
            var bar = new EntityWithGuidPrimaryKey();
            var hash = bar.GetHashCode();
            bar.Id = Guid.NewGuid();
            Assert.Equal(hash, bar.GetHashCode());
        }

        [Fact]
        public void EqualsNullIsFalse() {
            var bar = new EntityWithGuidPrimaryKey();
            Assert.False(bar.Equals(null));
        }

        [Fact]
        public void EqualsNonBarIsFalse() {
            var bar = new EntityWithGuidPrimaryKey();
            Assert.False(bar.Equals(new Foo()));
        }

        [Fact]
        public void NonSamePrimaryKeyNotEqual() {
            var bar = new EntityWithGuidPrimaryKey { Id = Guid.NewGuid() };
            var otherBar = new EntityWithGuidPrimaryKey { Id = Guid.NewGuid() };
            Assert.False(bar.Equals(otherBar));
            Assert.False(otherBar.Equals(bar));
        }

        [Fact]
        public void SamePrimaryKeyEqual() {
            var guid = Guid.NewGuid();
            var bar = new EntityWithGuidPrimaryKey { Id = guid };
            var otherBar = new EntityWithGuidPrimaryKey { Id = guid };
            Assert.True(bar.Equals(otherBar));
            Assert.True(otherBar.Equals(bar));
        }

        [Fact]
        public void SameInstanceIsEqual() {
            var bar = new EntityWithGuidPrimaryKey { Id = Guid.NewGuid() };
            Assert.True(bar.Equals(bar));
        }
    }
}