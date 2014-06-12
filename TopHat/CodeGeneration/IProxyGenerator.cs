namespace TopHat.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.Collections.Generic;

    using TopHat.Configuration;

    internal interface IProxyGenerator {
        IEnumerable<CodeTypeDeclaration> GenerateProxies(CodeGeneratorConfig codeGeneratorConfig, IDictionary<Type, IMap> maps);
    }
}