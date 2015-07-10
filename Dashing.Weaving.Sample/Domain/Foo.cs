namespace Dashing.Weaving.Sample.Domain {
    using System;

    public class Foo {
        public int FooId { get; set; }

        public string Name { get; set; }

        public bool IsBar { get; set; }

        public bool? IsRah { get; set; }

        public FooType Type { get; set; }
    }

    public enum FooType {
        One,
        Two,
        Three
    }

    public class Bar {
        public int Id { get; set; }

        public string Name { get; set; }

        public Foo Foo { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? SomethingOrTother { get; set; }
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