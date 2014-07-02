namespace TopHat.CodeGeneration {
    using System.CodeDom;

    internal class ProxyGeneratorResult {
        public CodeTypeDeclaration[] ProxyTypes { get; set; }

        public CodeNamespaceImport[] NamespaceImports { get; set; }

        public string[] ReferencedAssemblyLocations { get; set; }
    }
}