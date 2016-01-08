namespace Dashing.Configuration {
    using System.Configuration;
    using System.Data.Common;

    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public class DefaultConfiguration : ConfigurationBase {
        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings)
            : this(connectionStringSettings, new DefaultConvention()) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, IConvention mappingConvention)
            : this(connectionStringSettings, mappingConvention, new SqlEngine(new DialectFactory().Create(connectionStringSettings))) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, IEngine engine)
            : this(connectionStringSettings, new DefaultConvention(), engine) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, IConvention mappingConvention, IEngine engine)
            : base(
                engine,
                connectionStringSettings,
                DbProviderFactories.GetFactory(connectionStringSettings.ProviderName),
                new DefaultMapper(mappingConvention),
                new DefaultSessionFactory()) {
        }
    }
}