namespace Dashing.ElasticSearch {
    using Nest;

    public interface IElasticClientFactory {
        IElasticClient Create();
    }
}