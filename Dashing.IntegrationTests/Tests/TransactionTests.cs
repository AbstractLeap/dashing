namespace Dashing.IntegrationTests.Tests {
    public class TransactionTests {
        //[Theory]
        //[MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        //public void TestTransactioning(TestSessionWrapper wrapper) {
        //    // scrap the current transaction
        //    wrapper.Session.Complete();
        //    wrapper.Session.Dispose();

        //    using (var session = wrapper.Session.Configuration.BeginSession()) {
        //        session.Insert(new User { Username = "james", EmailAddress = "james@polylytics.com" });
        //        session.Complete();
        //    }

        //    using (var session = wrapper.Session.Configuration.BeginSession()) {
        //        Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
        //        session.Delete<User>(u => u.Username == "james");
        //        Assert.Null(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
        //    }

        //    using (var session = wrapper.Session.Configuration.BeginSession()) {
        //        Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
        //    }

        //    wrapper.Session = wrapper.Session.Configuration.BeginSession();
        //}
    }
}