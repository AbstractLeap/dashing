namespace Dashing.Weaving.Sample.Domain {
    public class Foo {
        public int FooId { get; set; }

        public string Name { get; set; }

        public bool IsBar { get; set; }

        public bool? IsRah { get; set; }
    }

    public class Bar {
        public int Id { get; set; }

        public string Name { get; set; }

        public Foo Foo { get; set; }
    }

    public class IveGotMethods {
        public int Id { get; set; }

        public string Name { get; set; }

        public override int GetHashCode() {
            return 42;
        }

        public override bool Equals(object obj) {
            return obj == null;
        }
    }
}