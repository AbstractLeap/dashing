using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Proxy
{
    public interface IBehaviourFactory
    {
        IBehaviour GetBehaviour();

        string Name { get; }
    }
}