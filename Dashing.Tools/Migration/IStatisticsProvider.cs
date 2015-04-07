namespace Dashing.Tools.Migration {
    using System.Collections.Generic;

    using Dashing.Configuration;

    public interface IStatisticsProvider {
        IDictionary<string, Statistics> GetStatistics(IEnumerable<IMap> fromMaps);
    }
}