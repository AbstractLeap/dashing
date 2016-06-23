namespace Dashing.IntegrationTests.Tests {
    using System;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class EnumeratingTests {
        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public void TestSingleAndFirst(TestSessionWrapper wrapper) {
            // now fetch them
            var t1 = wrapper.Session.Query<User>().First();
            Assert.Equal("User_0", t1.Username);

            var t2 = wrapper.Session.Query<User>().First(u => u.Username == "User_1");
            Assert.Equal("User_1", t2.Username);

            Assert.Throws<InvalidOperationException>(() => wrapper.Session.Query<User>().Single());

            var t3 = wrapper.Session.Query<User>().Single(u => u.Username == "User_2");
            Assert.Equal("User_2", t3.Username);

            var t4 = wrapper.Session.Query<User>().FirstOrDefault();
            Assert.Equal("User_0", t1.Username);

            var t5 = wrapper.Session.Query<User>().FirstOrDefault(u => u.Username == "Boo");
            Assert.Null(t5);

            var t6 = wrapper.Session.Query<User>().SingleOrDefault(u => u.Username == "Boo");
            Assert.Null(t6);
        }
    }
}