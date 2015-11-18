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
            : this(connectionStringSettings, mappingConvention, new DefaultSessionFactory()) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, ISessionFactory sessionFactory)
            : this(connectionStringSettings, new DefaultConvention(), sessionFactory) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, IConvention mappingConvention, ISessionFactory sessionFactory)
            : base(
                new SqlEngine(new DialectFactory().Create(connectionStringSettings)),
                connectionStringSettings,
                DbProviderFactories.GetFactory(connectionStringSettings.ProviderName),
                new DefaultMapper(mappingConvention),
                sessionFactory) {
        }
    }
}