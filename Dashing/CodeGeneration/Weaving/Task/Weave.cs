namespace Dashing.CodeGeneration.Weaving.Task {
    using Microsoft.Build.Framework;

    public class Weave : Microsoft.Build.Utilities.Task {
        /// <summary>
        /// The fullname of the type that contains the configuration
        /// </summary>
        [Required]
        public string ConfigurationFullName { get; set; }
        
        /// <summary>
        /// The path to the assembly that contains the configuration
        /// </summary>
        [Required]
        public string ConfigurationAssemblyPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// see https://github.com/dotnet/MVPSummitHackathon2016/blob/master/SampleTargets.PackerTarget/Packer.cs
        ///     https://github.com/bling/dependencypropertyweaver/blob/master/src/DependencyPropertyWeaver/DependencyPropertyWeaverTask.cs
        ///     https://github.com/dotnet/MVPSummitHackathon2016/blob/master/dotnet-packer/Program.cs
        /// </remarks>
        /// <returns></returns>
        public override bool Execute() {
            
        }
    }
}