using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Configuration {
    public class Index {
        public Index() {
            this.Columns = new List<IColumn>();
        }
        public IMap Map { get; set; }

        public ICollection<IColumn> Columns { get; set; }

        public bool IsUnique { get; set; }
    }
}
