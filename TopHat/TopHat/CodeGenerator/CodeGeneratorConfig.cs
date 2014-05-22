using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGenerator
{
    public class CodeGeneratorConfig
    {
        public CodeGeneratorConfig()
        {
            this.Namespace = "TopHat.Generated";
            this.ForeignKeyAccessClassSuffix = "_FK";
            this.ForeignKeyAccessEntityFieldSuffix = "_FKEntity";
        }

        /// <summary>
        /// Indicates that classes will be created with extra fields matching the underlying foreign key column name
        /// enabling resolving of foreign key ids
        /// </summary>
        public bool GenerateForeignKeyAccessClasses { get; set; }

        /// <summary>
        /// Indicates that classes will be generated that enable tracking of changes
        /// </summary>
        public bool GenerateChangeTrackingClasses { get; set; }

        public string Namespace { get; set; }

        public string ForeignKeyAccessClassSuffix { get; set; }

        public string ForeignKeyAccessEntityFieldSuffix { get; set; }

        public bool GenerateSource { get; set; }

        public bool GenerateAssembly { get; set; }
    }
}