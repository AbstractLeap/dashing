namespace Dashing.IntegrationTests.TestDomain {
    using System;
    using System.Collections.Generic;

    public class Blog {
        public Blog() {
            this.CreateDate = DateTime.Now;
            this.Posts = new List<Post>();
        }

        public long BlogId { get; set; }

        public string Title { get; set; }

        public DateTime CreateDate { get; set; }

        public string Description { get; set; }

        public IList<Post> Posts { get; set; }
    }
}