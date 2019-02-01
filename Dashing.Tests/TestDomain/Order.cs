namespace Dashing.Tests.TestDomain {
    public class Order {
        public int OrderId { get; set; }

        public Delivery Delivery { get; set; }

        public Customer Customer { get; set; }
    }
}