using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dashing.Configuration;

namespace Dashing.Tools.ReverseEngineering {
    internal interface IReverseEngineeringConfiguration : IConfiguration {
        void AddMap(Type type, IMap map);
    }
}
