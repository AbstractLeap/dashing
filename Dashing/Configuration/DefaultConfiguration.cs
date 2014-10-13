namespace Dashing.Configuration {
    using System.Configuration;
    using System.Data.Common;

    using Dashing.CodeGeneration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public class DefaultConfiguration : ConfigurationBase {
        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings)
            : this(connectionStringSettings, new DefaultConvention(), new CodeGeneratorConfig()) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, IConvention mappingConvention)
            : this(connectionStringSettings, mappingConvention, new CodeGeneratorConfig()) {
        }

        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings, CodeGeneratorConfig codeGeneratorConfig)
            : this(connectionStringSettings, new DefaultConvention(), codeGeneratorConfig) {
        }

        public DefaultConfiguration(
            ConnectionStringSettings connectionStringSettings,
            IConvention mappingConvention,
            CodeGeneratorConfig codeGeneratorConfig)
            : base(
                new SqlEngine(new DialectFactory().Create(connectionStringSettings)),
                connectionStringSettings,
                DbProviderFactories.GetFactory(connectionStringSettings.ProviderName),
                new DefaultMapper(mappingConvention),
                new DefaultSessionFactory(),
                new CodeGenerator(codeGeneratorConfig, new ProxyGenerator(), new DapperWrapperGenerator())) {
        }
    }
}