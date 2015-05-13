namespace Dashing.Testing.Tests {
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;

    public class MockSelectQueryTests {
        public IEnumerable<TestClass> Examples {
            get {
                yield return new TestClass(1, "Foo", false);
                yield return new TestClass(2, "Bar", false);
                yield return new TestClass(3, "Ape", true);
                yield return new TestClass(4, "Fizz", true);
                yield return new TestClass(5, "Buzz", true);
            }
        }

        [Fact]
        public void SelectQueryCanWhereAndSkipAndTakeAndOrder() {
            var target = new MockSelectQuery<TestClass>(this.Examples.ToList());

            var actual = target.Where(t => true).Where(t => t.IsCool).OrderBy(t => t.Name).Skip(1).Take(2).ToArray();

            Assert.NotNull(actual);
            Assert.Equal(2, actual.Count());
            Assert.Equal("Buzz", actual.ElementAt(0).Name);
            Assert.Equal("Fizz", actual.ElementAt(1).Name);
        }

        public class TestClass {
            public TestClass(int testClassId, string name, bool isCool) {
                this.TestClassId = testClassId;
                this.Name = name;
                this.IsCool = isCool;
            }

            public virtual int TestClassId { get; set; }

            public virtual string Name { get; set; }

            public virtual bool IsCool { get; set; }
        }
    }
}