namespace Dashing.Weaving.Tests {
    using System;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;
    using Dashing.Weaving.Sample.Domain.Tracking;

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
            Assert.False((bool)fooAsTracked.GetOldValue("IsBar"));
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
            Assert.Null(fooAsTracked.GetOldValue("IsRah"));
        }

        [Fact]
        public void ChangingNullableToNullWorks() {
            var foo = new Foo { IsRah = false };
            var fooAsTracked = (ITrackedEntity)foo;
            fooAsTracked.EnableTracking();
            foo.IsRah = null;
            Assert.Equal(new[] { "IsRah" }, fooAsTracked.GetDirtyProperties());
            Assert.False((bool)fooAsTracked.GetOldValue("IsRah"));
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
            Assert.True(barAsTracked.IsTrackingEnabled());
            Assert.False((bool)bar.GetType().GetField("__Foo_IsDirty", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bar));
            Assert.False(bar.Equals(null));
            bar.Foo = null;
            Assert.True((bool)bar.GetType().GetField("__Foo_IsDirty", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bar));
            Assert.Single(barAsTracked.GetDirtyProperties());
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
            Assert.Single(tingAsTracked.GetDirtyProperties());
            Assert.True(tingAsTracked.GetDirtyProperties().First() == "EntityWithStringPrimaryKey");
            Assert.Equal("Foo", ((EntityWithStringPrimaryKey)tingAsTracked.GetOldValue("EntityWithStringPrimaryKey")).Id);
        }

        [Fact]
        public void SetToNonNullOnNonFetchedButNotNullStringPkProperty()
        {
            var ting = new EntityReferencingEntityWithPrimaryKey();
            var tingAsTracked = (ITrackedEntity)ting;
            ting.GetType().GetField("EntityWithStringPrimaryKeyId").SetValue(ting, "Foo");
            tingAsTracked.EnableTracking();
            ting.EntityWithStringPrimaryKey = new EntityWithStringPrimaryKey { Id = "Bar" };
            Assert.Single(tingAsTracked.GetDirtyProperties());
            Assert.True(tingAsTracked.GetDirtyProperties().First() == "EntityWithStringPrimaryKey");
            Assert.Equal("Foo", ((EntityWithStringPrimaryKey)tingAsTracked.GetOldValue("EntityWithStringPrimaryKey")).Id);
        }

        [Fact]
        public void PropEqualNotDirty() {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References {
                                                IntPk = new IntPk { Id = 1 },
                                                LongPk = new LongPk { Id = 2 },
                                                GuidPk = new GuidPk { Id = Guid.NewGuid() },
                                                StringPk = new StringPk {  Id = "goo" },
                                                Bar = "bar",
                                                Foo = 12,
                                                G = guid
            };
            //@formatter:on
            
            var tracked = (ITrackedEntity)references;
            tracked.EnableTracking();

            references.IntPk = new IntPk { Id = 1 };
            references.LongPk = new LongPk { Id = 2 };
            references.GuidPk = new GuidPk { Id = references.GuidPk.Id };
            references.StringPk = new StringPk { Id = "goo" };
            references.Bar = "bar";
            references.Foo = 12;
            references.G = Guid.Parse(guid.ToString());

            Assert.Empty(tracked.GetDirtyProperties());
        }

        [Fact]
        public void PropNotEqualDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References
                             {
                                 IntPk = new IntPk { Id = 1 },
                                 LongPk = new LongPk { Id = 2 },
                                 GuidPk = new GuidPk { Id = Guid.NewGuid() },
                                 StringPk = new StringPk { Id = "goo" },
                                 Bar = "bar",
                                 Foo = 12,
                                 G = guid
                             };
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            tracked.EnableTracking();

            references.IntPk = new IntPk { Id = 2 };
            references.LongPk = new LongPk { Id = 3 };
            references.GuidPk = new GuidPk { Id = Guid.NewGuid() };
            references.StringPk = new StringPk { Id = "foo" };
            references.Bar = "car";
            references.Foo = 13;
            references.G = Guid.NewGuid();

            Assert.Equal(7, tracked.GetDirtyProperties().Count());
        }

        [Fact]
        public void FieldEqualNotDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References
                             {
                                 Bar = "bar",
                                 Foo = 12,
                                 G = guid
                             };
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            references.GetType().GetField("IntPkId").SetValue(references, 1);
            references.GetType().GetField("LongPkId").SetValue(references, 2L);
            references.GetType().GetField("GuidPkId").SetValue(references, guid);
            references.GetType().GetField("StringPkId").SetValue(references, "goo");

            tracked.EnableTracking();

            references.IntPk = new IntPk { Id = 1 };
            references.LongPk = new LongPk { Id = 2 };
            references.GuidPk = new GuidPk { Id = guid };
            references.StringPk = new StringPk { Id = "goo" };
            references.Bar = "bar";
            references.Foo = 12;
            references.G = Guid.Parse(guid.ToString());

            Assert.Empty(tracked.GetDirtyProperties());
        }

        [Fact]
        public void FieldNotEqualDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References
                             {
                                 Bar = "bar",
                                 Foo = 12,
                                 G = guid
                             };
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            references.GetType().GetField("IntPkId").SetValue(references, 2);
            references.GetType().GetField("LongPkId").SetValue(references, 3L);
            references.GetType().GetField("GuidPkId").SetValue(references, Guid.NewGuid());
            references.GetType().GetField("StringPkId").SetValue(references, "boo");

            tracked.EnableTracking();

            references.IntPk = new IntPk { Id = 1 };
            references.LongPk = new LongPk { Id = 2 };
            references.GuidPk = new GuidPk { Id = guid };
            references.StringPk = new StringPk { Id = "goo" };
            references.Bar = "car";
            references.Foo = 13;
            references.G = Guid.NewGuid();

            Assert.Equal(7, tracked.GetDirtyProperties().Count());
        }

        [Fact]
        public void PropNullValueNotNullDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References( );
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            tracked.EnableTracking();

            references.IntPk = new IntPk { Id = 1 };
            references.LongPk = new LongPk { Id = 2 };
            references.GuidPk = new GuidPk { Id = guid };
            references.StringPk = new StringPk { Id = "goo" };
            references.Bar = "car";
            references.Foo = 13;
            references.G = Guid.NewGuid();

            Assert.Equal(7, tracked.GetDirtyProperties().Count());
        }

        [Fact]
        public void ValueNullPropNullNotDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References();
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            tracked.EnableTracking();

            references.IntPk = null;
            references.LongPk = null;
            references.GuidPk = null;
            references.StringPk = null;
            references.Bar = null;

            Assert.Empty(tracked.GetDirtyProperties());
        }

        [Fact]
        public void PropNotNulLValueNullDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References
                             {
                                 IntPk = new IntPk { Id = 1 },
                                 LongPk = new LongPk { Id = 2 },
                                 GuidPk = new GuidPk { Id = Guid.NewGuid() },
                                 StringPk = new StringPk { Id = "goo" },
                                 Bar = "bar"
                             };
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            tracked.EnableTracking();

            references.IntPk = null;
            references.LongPk = null;
            references.GuidPk = null;
            references.StringPk = null;
            references.Bar = null;

            Assert.Equal(5, tracked.GetDirtyProperties().Count());
        }

        [Fact]
        public void FieldNotNullValueNullDirty()
        {
            //@formatter:off
            var guid = Guid.NewGuid();
            var references = new References
                             {
                                 Bar = "bar",
                                 Foo = 12,
                                 G = guid
                             };
            //@formatter:on

            var tracked = (ITrackedEntity)references;
            references.GetType().GetField("IntPkId").SetValue(references, 2);
            references.GetType().GetField("LongPkId").SetValue(references, 3L);
            references.GetType().GetField("GuidPkId").SetValue(references, Guid.NewGuid());
            references.GetType().GetField("StringPkId").SetValue(references, "boo");

            tracked.EnableTracking();

            references.IntPk = null;
            references.LongPk = null;
            references.GuidPk = null;
            references.StringPk = null;

            Assert.Equal(4, tracked.GetDirtyProperties().Count());
        }
    }
}