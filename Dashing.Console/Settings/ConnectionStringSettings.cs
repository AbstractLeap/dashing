namespace Dashing.Console.Settings {
    internal class ConnectionStringSettings {
        public string ProviderName { get; set; }

        public string ConnectionString { get; set; }

        public System.Configuration.ConnectionStringSettings ToSystem() {
            return new System.Configuration.ConnectionStringSettings("Default", this.ConnectionString, this.ProviderName);
        }
    }
}