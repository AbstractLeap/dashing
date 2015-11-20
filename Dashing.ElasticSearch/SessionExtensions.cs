namespace Dashing.ElasticSearch {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nest;

    public static class SessionExtensions {
        public static IElasticClientFactory ElasticClientFactory { get; set; }

        public static IEnumerable<T> Elastic<T>(this ISession session, Func<SearchDescriptor<T>, SearchDescriptor<T>> descriptor) where T : class {
            // TODO: this is a bad pattern, we should put some dependency resolution into Dashing so that you can ask the Session for a component
            if (ElasticClientFactory == null) {
                throw new NullReferenceException("The ClientFactory must be explicitly set prior to use");
            }

            return ElasticClientFactory.Create().Search(descriptor).Hits.Select(result => result.Source);
        }
    }
}