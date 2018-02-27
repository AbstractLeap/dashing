namespace Dashing.Tests.TestDomain {
    public class OneToOneRight {
        public virtual int OneToOneRightId { get; set; }

        public virtual OneToOneLeft Left { get; set; }

        public virtual string Name { get; set; }
    }
}