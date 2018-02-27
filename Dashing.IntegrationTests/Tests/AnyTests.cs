namespace Dashing.IntegrationTests.Tests {
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class AnyTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task AnyAsyncTrueWorks(TestSessionWrapper wrapper) {
            Assert.True(await wrapper.Session.Query<User>().Where(u => u.UserId == 8).AnyAsync());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task AnyAsyncFalseWorks(TestSessionWrapper wrapper) {
            Assert.False(await wrapper.Session.Query<User>().Where(u => u.UserId == 2000).AnyAsync());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task AnyAsyncWhereTrueWorks(TestSessionWrapper wrapper) {
            Assert.True(await wrapper.Session.Query<User>().AnyAsync(u => u.UserId == 8));
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task AnyAsyncWhereFalseWorks(TestSessionWrapper wrapper) {
            Assert.False(await wrapper.Session.Query<User>().AnyAsync(u => u.UserId == 2000));
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void AnyTrueWorks(TestSessionWrapper wrapper) {
            Assert.True(wrapper.Session.Query<User>().Where(u => u.UserId == 8).Any());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void AnyFalseWorks(TestSessionWrapper wrapper) {
            Assert.False(wrapper.Session.Query<User>().Where(u => u.UserId == 2000).Any());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void AnyWhereTrueWorks(TestSessionWrapper wrapper) {
            Assert.True(wrapper.Session.Query<User>().Any(u => u.UserId == 8));
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void AnyWhereFalseWorks(TestSessionWrapper wrapper) {
            Assert.False(wrapper.Session.Query<User>().Any(u => u.UserId == 2000));
        }
    }
}