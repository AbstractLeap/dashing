namespace Dashing.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.Collections.Generic;

    using Dashing.Configuration;

    /// <summary>
    /// Generate the wrapper around several Dapper methods
    /// </summary>
    public interface IDapperWrapperGenerator {
        /// <summary>
        /// Generate the declarations for the dapper wrapper class
        /// </summary>
        CodeTypeDeclaration GenerateDapperWrapper(CodeGeneratorConfig config, IEnumerable<IMap> maps, Func<Type, IMap> getMap);
    }
}