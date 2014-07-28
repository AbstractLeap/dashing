namespace Dashing.Configuration {
    using System.Configuration;
    using System.Data.Common;

    using Dashing.CodeGeneration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public class DefaultConfiguration : ConfigurationBase {
        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings)
            : base(new SqlEngine(new DialectFactory().Create(connectionStringSettings)), connectionStringSettings, DbProviderFactories.GetFactory(connectionStringSettings.ProviderName), new DefaultMapper(new DefaultConvention()), new DefaultSessionFactory(), new CodeGenerator(new CodeGeneratorConfig(), new ProxyGenerator(), new DapperWrapperGenerator())) { }
    }
}