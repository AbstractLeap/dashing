namespace Dashing.IntegrationTests.Setup {
    public class TestSessionWrapper {
        public TestSessionWrapper(ISession session) {
            this.Session = session;
        }

        public ISession Session { get; private set; }

        public string DatabaseName {
            get {
                return DatabaseInitializer.DatabaseName;
            }
        }
    }
}