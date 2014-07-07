namespace Dashing {
    using System.Configuration;

    using Dashing.Configuration;

    public static class NeedToDash {
        public static MutableConfiguration Configure(ConnectionStringSettings connectionString) {
            return new MutableConfiguration(connectionString);
        }
    }
}