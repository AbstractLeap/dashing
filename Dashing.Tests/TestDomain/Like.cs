using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Tests.TestDomain {
    public class Like {
        public virtual int LikeId { get; set; }

        public virtual User User { get; set; }

        public virtual Comment Comment { get; set; }
    }
}
