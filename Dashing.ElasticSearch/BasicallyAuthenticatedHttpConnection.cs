namespace Dashing.ElasticSearch {
    using System;
    using System.Net;
    using System.Text;

    using Elasticsearch.Net.Connection;
    using Elasticsearch.Net.Connection.Configuration;

    public class BasicallyAuthenticatedHttpConnection : HttpConnection {
        private readonly string authorizationHeader;

        public BasicallyAuthenticatedHttpConnection(
            string username, string password, IConnectionConfigurationValues settings)
            : base(settings) {
            this.authorizationHeader = string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));
        }

        protected override HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data, IRequestConfiguration requestSpecificConfig) {
            var request = base.CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
            request.Headers["Authorization"] = this.authorizationHeader;
            return request;
        }
    }
}