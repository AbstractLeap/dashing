namespace Dashing.ElasticSearch {
    using System;

    using Elasticsearch.Net.Connection;

    using Nest;

    public class ElasticClientFactory : IElasticClientFactory {
        private IElasticClient client;

        public ElasticClientFactory(string uri, string defaultIndex, string username, string password) {
            var uriuri = new Uri(uri);
            var settings = new ConnectionSettings(uriuri, defaultIndex);
            var connection = new BasicallyAuthenticatedHttpConnection(username, password, new ConnectionConfiguration(uriuri));
            this.CreateClient(settings, connection, null, null);
        }

        public ElasticClientFactory(
            IConnectionSettingsValues settings = null,
            IConnection connection = null,
            ITransport transport = null,
            INestSerializer serializer = null) {
            this.CreateClient(settings, connection, transport, serializer);
        }

        private void CreateClient(IConnectionSettingsValues settings, IConnection connection, ITransport transport, INestSerializer serializer) {
            this.client = new ElasticClient(settings, connection, serializer, transport);
        }

        public IElasticClient Create() {
            return this.client;
        }
    }
}