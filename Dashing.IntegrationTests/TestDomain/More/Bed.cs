namespace Dashing.IntegrationTests.TestDomain.More {
    public class Bed {
        public long BedId { get; set; }

        public RoomSlot RoomSlot { get; set; }

        public Booking Booking { get; set; }
    }
}