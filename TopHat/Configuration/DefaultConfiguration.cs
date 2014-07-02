namespace TopHat.Configuration {
    using System.Configuration;
    using TopHat.CodeGeneration;
    using TopHat.Engine;

    /// <summary>
    ///     The default configuration.
    /// </summary>
    public class DefaultConfiguration : ConfigurationBase {
        private System.Configuration.ConnectionStringSettings connectionString;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultConfiguration" /> class.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        public DefaultConfiguration(IEngine engine, string connectionString)
            : base(
                engine, 
                connectionString, 
                new DefaultMapper(new DefaultConvention()), 
                new DefaultSessionFactory(), 
                new CodeGenerator(new CodeGeneratorConfig(), new ProxyGenerator())) { }

        public DefaultConfiguration(ConnectionStringSettings connectionString)
            : base(
            connectionString,
                new DefaultMapper(new DefaultConvention()),
                new DefaultSessionFactory(),
                new CodeGenerator(new CodeGeneratorConfig(), new ProxyGenerator()))
        {
            
        }
    }
}