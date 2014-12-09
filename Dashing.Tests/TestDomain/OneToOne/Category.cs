namespace Dashing.Tests.TestDomain.OneToOne {
    using System.Collections.Generic;

    public class Category {
        public virtual int CategoryId { get; set; }

        public virtual Category Parent { get; set; }

        public virtual IList<Category> Children { get; set; }

        public virtual string Name { get; set; }
    }
}