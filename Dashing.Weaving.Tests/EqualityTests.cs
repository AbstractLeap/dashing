namespace Dashing.Weaving.Tests {
    using System.Runtime.CompilerServices;

    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    [Collection("Weaving Tests")]
    public class EqualityTests {
        [Fact]
        public void GetHashCodeReturnsIdFactor() {
            var bar = new Bar { Id = 3 };
            Assert.Equal(17 * 3, bar.GetHashCode());
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

        [Fact(Skip = "Need to re-write all instances of == and != in calling dlls for this to work")]
        public void EqualityGetsOverridden() {
            var bar = new Bar { Id = 1 };
            var bar2 = new Bar { Id = 2 };
            var anotherBar1 = new Bar { Id = 1 };
            Bar nullBar1 = null;
            Bar nullBar2 = null;

            Assert.True(nullBar1 == nullBar2);
            Assert.False(nullBar1 != nullBar2);
            Assert.False(bar == nullBar1);
            Assert.True(bar != nullBar1);
            Assert.False(nullBar2 == bar);
            Assert.True(nullBar2 != bar);
            Assert.True(bar == anotherBar1);
            Assert.False(bar != anotherBar1);
            Assert.False(bar == bar2);
            Assert.True(bar != bar2);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(bar == bar);
            Assert.False(bar != bar);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Fact]
        public void OpEqualityTests() {
            var c = new C();
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(c == c);
#pragma warning restore CS1718 // Comparison made to same variable

            var c2 = new C();
            Assert.False(c == c2);

            var b = new B { Id = 1 };
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(b == b);
#pragma warning restore CS1718 // Comparison made to same variable

            var b2 = new B { Id = 2 };
            var bAnother1 = new B { Id = 1 };
            Assert.False(b == b2);
            Assert.True(b == bAnother1);

            var a = new A { Id = 1 };
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(a == a);
#pragma warning restore CS1718 // Comparison made to same variable

            var a2 = new A { Id = 2 };
            var aAnother1 = new A { Id = 1 };
            Assert.False(a == a2);
            Assert.True(a == aAnother1);
        }
    }

    public class C {
        public int Id { get; set; }
    }

#pragma warning disable 660,661
    public class B : C {
#pragma warning restore 660,661
        public static bool operator ==(B left, B right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if ((object)left == null || (object)right == null) {
                return false;
            }

            return left.Id == right.Id;
        }

        public static bool operator !=(B left, B right) {
            return !(left == right);
        }
    }

    public class A : B {
    }
}