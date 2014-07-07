namespace Dashing.Configuration {
    using System.Configuration;

    using Dashing.CodeGeneration;
    using Dashing.Engine;

    public class DefaultConfiguration : ConfigurationBase {
        public DefaultConfiguration(ConnectionStringSettings connectionStringSettings)
            : base(new SqlEngine(new DialectFactory().Create(connectionStringSettings)), connectionStringSettings, new DefaultMapper(new DefaultConvention()), new DefaultSessionFactory(), new CodeGenerator(new CodeGeneratorConfig(), new ProxyGenerator())) { }
    }
}