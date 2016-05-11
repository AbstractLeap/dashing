namespace Dashing.IntegrationTests.TestDomain {
    public class User {
        public long UserId { get; set; }

        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public bool IsEnabled { get; set; }

        public decimal HeightInMeters { get; set; }
    }
}