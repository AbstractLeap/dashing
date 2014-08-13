using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Configuration {
    public interface ISeeder {
        void Seed(ISession session);
    }
}
