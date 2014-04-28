namespace TopHat.Configuration
{
    public interface IConfiguration
    {
        /// <summary>
        /// If set to true will always return tracked entities from queries
        /// </summary>
        bool AlwaysTrackEntities { get; set; }

        /// <summary>
        /// Specifies the default schema name
        /// </summary>
        string DefaultSchema { get; set; }

        /// <summary>
        /// The object to table mappings
        /// </summary>
        IMapping Mapping { get; }

        /// <summary>
        /// Run the configuration
        /// </summary>
        Mapper.Mapper Configure();
    }
}