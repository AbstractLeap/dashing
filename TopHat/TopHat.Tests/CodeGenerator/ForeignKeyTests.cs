using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;
using Cg = TopHat.CodeGenerator;

namespace TopHat.Tests.CodeGenerator
{
    public class ForeignKeyTests
    {
        private Mock<IEngine> engine = new Mock<IEngine>();

        [Fact]
        public void FKTest()
        {
            var config = new CustomConfig(this.engine.Object);
            var codeGenerator = new Cg.CodeGenerator();
            var codeConfig = new Cg.CodeGeneratorConfig();
            codeConfig.GenerateAssembly = true;
            codeConfig.GenerateSource = true;
            codeGenerator.Generate(config, codeConfig);
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