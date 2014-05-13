using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Proxy.ChangeTracking
{
    public class ChangeTrackingBehaviourFactory : IBehaviourFactory
    {
        public IBehaviour GetBehaviour()
        {
            return new ChangeTrackingBehaviour();
        }

        public string Name
        {
            get { return "ChangeTracking"; }
        }
    }
}