namespace Dashing.Weaving.Tests {
    using System;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    [Collection("Weaving Tests")]
    public class TrackingTests {
        [Fact]
        public void ImplementsITrackedEntity() {
            var foo = new Foo();
            Assert.True(foo is ITrackedEntity);
        }

        [Fact]
        public void IsTrackingDisabledByDefault() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            Assert.False(fooAsTracked.IsTrackingEnabled());
        }

        [Fact]
        public void NotEnablingTrackingDoesNotMarkPropsAsDirty() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            foo.Name = "Bar";
            Assert.Empty(fooAsTracked.GetDirtyProperties());
            Assert.Throws<ArgumentOutOfRangeException>(() => fooAsTracked.GetOldValue("Name"));
        }

        [Fact]
        public void NoDirtyPropsToStartWith() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            Assert.Empty(fooAsTracked.GetDirtyProperties());
        }

        [Fact]
        public void EnableTrackingEnablesTracking() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            Assert.True(fooAsTracked.IsTrackingEnabled());
        }

        [Fact]
        public void SettingStringMakesDirty() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.Name = "Rah";
            Assert.True(fooAsTracked.GetDirtyProperties().First() == "Name");
            Assert.True(fooAsTracked.GetOldValue("Name") == null);
        }

        [Fact]
        public void NotChangingStringValueMakesNotDirty() {
            var foo = new Foo { Name = "Foo" };
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.Name = "Foo";
            Assert.Empty(fooAsTracked.GetDirtyProperties());
            Assert.Throws<ArgumentOutOfRangeException>(() => fooAsTracked.GetOldValue("Name"));
        }

        [Fact]
        public void ChangeBoolWorks() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.IsBar = true;
            Assert.Equal(new[] { "IsBar" }, fooAsTracked.GetDirtyProperties());
            Assert.Equal(false, fooAsTracked.GetOldValue("IsBar"));
        }

        [Fact]
        public void NotChangingBoolWorks() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.IsBar = false;
            Assert.Empty(fooAsTracked.GetDirtyProperties());
            Assert.Throws<ArgumentOutOfRangeException>(() => fooAsTracked.GetOldValue("IsBar"));
        }

        [Fact]
        public void ChangingNullableWorks() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.IsRah = true;
            Assert.Equal(new[] { "IsRah" }, fooAsTracked.GetDirtyProperties());
            Assert.Equal(null, fooAsTracked.GetOldValue("IsRah"));
        }

        [Fact]
        public void ChangingNullableToNullWorks() {
            var foo = new Foo { IsRah = false };
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.IsRah = null;
            Assert.Equal(new[] { "IsRah" }, fooAsTracked.GetDirtyProperties());
            Assert.Equal(false, fooAsTracked.GetOldValue("IsRah"));
        }

        [Fact]
        public void ChangingMoreThanOnceDoesNotOverwriteOldValue() {
            var foo = new Foo { Name = "Foo" };
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.Name = "Rah";
            foo.Name = "Boo";
            Assert.True(fooAsTracked.GetDirtyProperties().First() == "Name");
            Assert.Equal("Foo", fooAsTracked.GetOldValue("Name"));
            Assert.Equal("Boo", foo.Name);
        }

        [Fact]
        public void DisablingTrackingResetsAllDirty() {
            var foo = new Foo { Name = "Foo" };
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.Name = "Rah";
            fooAsTracked.DisableTracking();
            Assert.Empty(fooAsTracked.GetDirtyProperties());
            Assert.Throws<ArgumentOutOfRangeException>(() => fooAsTracked.GetOldValue("Name"));
        }

        [Fact]
        public void SettingEnumMakesPropDirty() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.Type = FooType.Two;
            Assert.Equal(new[] { "Type" }, fooAsTracked.GetDirtyProperties());
        }

        [Fact]
        public void NotChangingEnumDoesNotSetDirty() {
            var foo = new Foo();
            var fooAsTracked = (ITrackedEntity)foo;
            foo.Type = FooType.Two;
            fooAsTracked.EnableTracking();
            foo.Type = FooType.Two;
            Assert.Empty(fooAsTracked.GetDirtyProperties());
        }

        [Fact]
        public void SetMethodNotOverridden() {
            var starship = new Starship();
            starship.Foo = true;
            Assert.True(starship.GetBar());
        }

        [Fact]
        public void SetToNullOnNonFetchedButNotNullProperty() {
            var barCheck = new Bar();
            barCheck.GetType().GetField("FooId").SetValue(barCheck, 1);
            Assert.Equal(1, barCheck.Foo.FooId);

            var bar = new Bar();
            var barAsTracked = (ITrackedEntity)bar;
            bar.GetType().GetField("FooId").SetValue(bar, 2);
            barAsTracked.EnableTracking();
            bar.Foo = null;
            Assert.Equal(1, barAsTracked.GetDirtyProperties().Count());
            Assert.True(barAsTracked.GetDirtyProperties().First() == "Foo");
            Assert.Equal(2, ((Foo)barAsTracked.GetOldValue("Foo")).FooId);
        }

        [Fact]
        public void SetToNullOnNonFetchedButNotNullStringPkProperty() {
            var check = new EntityReferencingEntityWithPrimaryKey();
            check.GetType().GetField("EntityWithStringPrimaryKeyId").SetValue(check, "Foo");
            Assert.Equal("Foo", check.EntityWithStringPrimaryKey.Id);

            var ting = new EntityReferencingEntityWithPrimaryKey();
            var tingAsTracked = (ITrackedEntity)ting;
            ting.GetType().GetField("EntityWithStringPrimaryKeyId").SetValue(ting, "Foo");
            tingAsTracked.EnableTracking();
            ting.EntityWithStringPrimaryKey = null;
            Assert.Equal(1, tingAsTracked.GetDirtyProperties().Count());
            Assert.True(tingAsTracked.GetDirtyProperties().First() == "EntityWithStringPrimaryKey");
            Assert.Equal("Foo", ((EntityWithStringPrimaryKey)tingAsTracked.GetOldValue("EntityWithStringPrimaryKey")).Id);
        }
    }
}