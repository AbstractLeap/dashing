namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    internal interface IProxyGenerator {
        ProxyGeneratorResult GenerateProxies(CodeGeneratorConfig codeGeneratorConfig, IDictionary<Type, IMap> maps);
    }
}