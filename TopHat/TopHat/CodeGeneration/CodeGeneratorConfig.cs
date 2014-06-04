namespace TopHat.CodeGeneration {
    public class CodeGeneratorConfig {
        public CodeGeneratorConfig() {
            this.Namespace = "TopHat.Generated";
            this.ForeignKeyAccessClassSuffix = "_FK";
            this.ForeignKeyAccessEntityFieldSuffix = "_FKEntity";
            this.TrackedClassSuffix = "_Tracked";
            this.MapperGenerationMaxRecursion = 8;
            this.SourceLocation = "generated.cs";
        }

        /// <summary>
        ///     Indicates that classes will be created with extra fields matching the underlying foreign key column name
        ///     enabling resolving of foreign key ids
        /// </summary>
        public bool GenerateForeignKeyAccessClasses { get; set; }

        /// <summary>
        ///     Indicates that classes will be generated that enable tracking of changes
        /// </summary>
        public bool GenerateChangeTrackingClasses { get; set; }

        public string Namespace { get; set; }

        public string ForeignKeyAccessClassSuffix { get; set; }

        public string ForeignKeyAccessEntityFieldSuffix { get; set; }

        public string TrackedClassSuffix { get; set; }

        public bool GenerateSource { get; set; }

        public string SourceLocation { get; set; }

        public bool GenerateAssembly { get; set; }

        public int MapperGenerationMaxRecursion { get; set; }
    }
}