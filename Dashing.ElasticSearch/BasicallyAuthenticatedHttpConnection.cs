using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net.Connection;

namespace Dashing.ElasticSearch {
    public class BasicallyAuthenticatedHttpConnection : HttpConnection {
        private string username;
        private string password;
        public BasicallyAuthenticatedHttpConnection(string username, string password, IConnectionConfigurationValues settings)
            : base(settings) {
                this.username = username;
                this.password = password;
        }

        protected override System.Net.HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data, Elasticsearch.Net.Connection.Configuration.IRequestConfiguration requestSpecificConfig) {
            var request = base.CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
            request.Credentials = new NetworkCredential(this.username, this.password);
            request.Headers["Authorization"] =
                "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(request.RequestUri.UserInfo));
            return request;
        }
    }
}
