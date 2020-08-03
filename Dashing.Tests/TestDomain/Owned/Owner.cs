namespace Dashing.Tests.TestDomain.Owned {
    public class Owner {
        public int Id { get; set; }

        public string Name { get; set; }

        public Owned Owned { get; set; }
    }
}