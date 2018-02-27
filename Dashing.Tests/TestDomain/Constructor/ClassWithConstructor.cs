namespace Dashing.Tests.TestDomain.Constructor {
    using System;

    public class ClassWithConstructor {
        public ClassWithConstructor() {
            this.CreatedDate = DateTime.UtcNow;
        }

        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}