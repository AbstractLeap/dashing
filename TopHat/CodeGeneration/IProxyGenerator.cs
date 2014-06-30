namespace TopHat.CodeGeneration {
    using System;
    using System.Collections.Generic;

    using TopHat.Configuration;

    internal interface IProxyGenerator {
        ProxyGeneratorResult GenerateProxies(CodeGeneratorConfig codeGeneratorConfig, IDictionary<Type, IMap> maps);
    }
}