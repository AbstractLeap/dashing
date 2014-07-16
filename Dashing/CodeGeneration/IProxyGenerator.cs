namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public interface IProxyGenerator {
        ProxyGeneratorResult GenerateProxies(CodeGeneratorConfig codeGeneratorConfig, IDictionary<Type, IMap> maps);
    }
}