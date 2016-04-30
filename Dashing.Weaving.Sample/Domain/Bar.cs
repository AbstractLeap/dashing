namespace Dashing.Weaving.Sample.Domain {
    using System;
    using System.Collections.Generic;

    public class Bar {
        public Bar() {
            this.Ducks = new[] { new Duck { Id = 1 } };
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Foo Foo { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? SomethingOrTother { get; set; }

        public IList<Duck> Ducks { get; set; }
    }
}