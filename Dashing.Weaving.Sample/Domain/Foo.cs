namespace Dashing.Weaving.Sample.Domain {
    using System.Collections.Generic;

    public class Foo {
        public int FooId { get; set; }

        public string Name { get; set; }

        public bool IsBar { get; set; }

        public bool? IsRah { get; set; }

        public FooType Type { get; set; }

        public IEnumerable<Bar> Bars { get; set; }
    }
}