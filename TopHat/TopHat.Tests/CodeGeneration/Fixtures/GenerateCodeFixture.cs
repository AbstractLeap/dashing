using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.CodeGeneration;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;

namespace TopHat.Tests.CodeGeneration.Fixtures
{
    public class GenerateCodeFixture
    {
        private Mock<IEngine> engine = new Mock<IEngine>();

        public IGeneratedCodeManager CodeManager { get; private set; }

        public GenerateCodeFixture()
        {
            // generate config and assembly
            IConfiguration config = new CustomConfig(this.engine.Object);
            var codeGenerator = new CodeGenerator();
            var codeConfig = new CodeGeneratorConfig();
            codeConfig.GenerateAssembly = true;
            codeGenerator.Generate(config, codeConfig);
            this.CodeManager = new GeneratedCodeManager();
            this.CodeManager.LoadCode(codeConfig);
        }

        private class CustomConfig : DefaultConfiguration
        {
            public CustomConfig(IEngine engine)
                : base(engine, string.Empty)
            {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}