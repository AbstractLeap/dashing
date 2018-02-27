namespace Dashing.Cli {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Migration;

    internal class NullStatisticsProvider : IStatisticsProvider {
        public IDictionary<string, Statistics> GetStatistics(IEnumerable<IMap> fromMaps) {
            return fromMaps.ToDictionary(m => m.Type.Name, m => new Statistics { HasRows = false });
        }
    }
}