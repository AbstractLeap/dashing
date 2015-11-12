namespace Dashing.Tests.CodeGeneration {
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    using Moq;

    using Xunit;

    public class TrackedEntityInspectorTests {
        [Fact]
        public void IsPropertyDirtyIs() {
            var foo = new Foo(new []{ "Name" }, new Dictionary<string, object>{ { "Name", "Mark" }});
            var inspector = new TrackedEntityInspector<Foo>(foo);
            Assert.True(inspector.IsPropertyDirty(f => f.Name));
        }

        [Fact]
        public void IsPropertyDirtyIsnt() {
            var foo = new Foo(new[] { "Name" }, new Dictionary<string, object> { { "Name", "Mark" } });
            var inspector = new TrackedEntityInspector<Foo>(foo);
            Assert.False(inspector.IsPropertyDirty(f => f.Bar));
        }

        [Fact]
        public void GetOldValueWorks() {
            var foo = new Foo(new[] { "Name" }, new Dictionary<string, object> { { "Name", "Mark" } });
            var inspector = new TrackedEntityInspector<Foo>(foo);
            Assert.Equal("Mark", inspector.GetOldValue(f => f.Name));
        }

        [Fact]
        public void SessionExtensionWorks() {
            var foo = new Foo(new[] { "Name" }, new Dictionary<string, object> { { "Name", "Mark" } });
            var inspector = SessionExtensions.Inspect(new Mock<ISession>().Object, foo);
            Assert.True(inspector.IsPropertyDirty(f => f.Name));
        }

        [Fact]
        public void GetNewValueWorks() {
            var foo = new Foo(new[] { "Name" }, new Dictionary<string, object> { { "Name", "Mark" } }) { Name = "James" };
            var inspector = SessionExtensions.Inspect(new Mock<ISession>().Object, foo);
            Assert.Equal("James", inspector.GetNewValue(f => f.Name));
        }

        [Fact]
        public void IsDirtyIfHasDirtyProps() {
            var foo = new Foo(new[] { "Name" }, new Dictionary<string, object> { { "Name", "Mark" } }) { Name = "James" };
            var inspector = SessionExtensions.Inspect(new Mock<ISession>().Object, foo);
            Assert.True(inspector.IsDirty());
        }

        [Fact]
        public void IsNotDirtyIfDoesNotHaveDirtyProps() {
            var foo = new Foo(new string[0], new Dictionary<string, object> ()) { Name = "James" };
            var inspector = SessionExtensions.Inspect(new Mock<ISession>().Object, foo);
            Assert.False(inspector.IsDirty());
        }
    }

    public class Foo : ITrackedEntity {
        private readonly string[] dirtyProps;

        private readonly IDictionary<string, object> oldValues;

        public int FooId { get; set; }

        public string Name { get; set; }

        public decimal Bar { get; set; }

        public Foo(string[] dirtyProps, IDictionary<string, object> oldValues) {
            this.dirtyProps = dirtyProps;
            this.oldValues = oldValues;
        }

        public void EnableTracking() {
            throw new System.NotImplementedException();
        }

        public void DisableTracking() {
            throw new System.NotImplementedException();
        }

        public bool IsTrackingEnabled() {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetDirtyProperties() {
            return this.dirtyProps ?? new string[0];
        }

        public object GetOldValue(string propertyName) {
            return this.oldValues[propertyName];
        }
    }
}