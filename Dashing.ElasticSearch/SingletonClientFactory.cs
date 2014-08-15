using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nest;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.Serialization;

namespace Dashing.ElasticSearch {
    public class SingletonClientFactory : IClientFactory {
        private IElasticClient client;

        public SingletonClientFactory(string uri, string defaultIndex) :
            this(new ConnectionSettings(new Uri(uri), defaultIndex), null, null) {

        }

        public SingletonClientFactory(IConnectionSettingsValues settings = null, IConnection connection = null, ITransport transport = null, INestSerializer serializer = null) {
            CreateClient(settings, connection, transport, serializer);
        }

        private void CreateClient(IConnectionSettingsValues settings, IConnection connection, ITransport transport, INestSerializer serializer) {
            this.client = new ElasticClient(settings, connection, serializer, transport);
        }

        public IElasticClient Create() {
            return this.client;
        }
    }
}
