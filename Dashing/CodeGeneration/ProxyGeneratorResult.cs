namespace Dashing.CodeGeneration {
    using System.CodeDom;

    public class ProxyGeneratorResult {
        public CodeTypeDeclaration[] ProxyTypes { get; set; }

        public CodeNamespaceImport[] NamespaceImports { get; set; }

        public string[] ReferencedAssemblyLocations { get; set; }
    }
}