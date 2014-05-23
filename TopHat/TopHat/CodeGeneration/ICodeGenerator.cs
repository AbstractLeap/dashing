using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;

namespace TopHat.CodeGeneration
{
    internal interface ICodeGenerator
    {
        void Generate(IConfiguration configuration, CodeGeneratorConfig generatorConfig);
    }
}