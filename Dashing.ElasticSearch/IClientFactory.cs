using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nest;

namespace Dashing.ElasticSearch {
    public interface IClientFactory {
        IElasticClient Create();
    }
}
