namespace Dashing.Tests.TestDomain {
    public class ThingThatReferencesOrderNullable {
        public int Id { get; set; }

        public Order Order { get; set; }
    }
}