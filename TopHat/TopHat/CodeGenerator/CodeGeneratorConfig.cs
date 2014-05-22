using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGenerator
{
    public class CodeGeneratorConfig
    {
        /// <summary>
        /// Indicates that classes will be created with extra fields matching the underlying foreign key column name
        /// enabling resolving of foreign key ids
        /// </summary>
        public virtual bool GenerateForeignKeyAccessClasses { get; set; }

        /// <summary>
        /// Indicates that classes will be generated that enable tracking of changes
        /// </summary>
        public virtual bool GenerateChangeTrackingClasses { get; set; }
    }
}