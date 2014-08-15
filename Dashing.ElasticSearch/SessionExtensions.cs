using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nest;
using Dashing;

namespace Dashing.ElasticSearch {
    public static class SessionExtensions {
        public static IClientFactory ClientFactory { get; set; }

        public static IEnumerable<T> Elastic<T>(this ISession session, Func<SearchDescriptor<T>, SearchDescriptor<T>> descriptor) where T : class {
            if (ClientFactory == null) {
                throw new NullReferenceException("The ClientFactory must be explicitly set prior to use");
            }

            var results = ClientFactory.Create().Search<T>(descriptor);
            foreach (var result in results.Hits) {
                yield return result.Source;
            }
        }
    }
}
