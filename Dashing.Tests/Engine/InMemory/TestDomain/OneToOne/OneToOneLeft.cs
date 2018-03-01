namespace Dashing.Tests.Engine.InMemory.TestDomain.OneToOne {
    public class OneToOneLeft {
        public virtual int OneToOneLeftId { get; set; }

        public virtual OneToOneRight Right { get; set; }

        public virtual string Name { get; set; }
    }
}