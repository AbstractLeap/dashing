namespace Dashing.IntegrationTests.TestDomain.More {
    using System.Collections.Generic;

    public class Booking {
        public long BookingId { get; set; }

        public bool IsFoo { get; set; }

        public IList<Bed> Beds { get; set; }
    }
}