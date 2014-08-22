namespace Dashing.Configuration {
    using System.Collections.Generic;

    public class Index {
        public Index() {
            this.Columns = new List<IColumn>();
        }

        public IMap Map { get; set; }

        public ICollection<IColumn> Columns { get; set; }

        public bool IsUnique { get; set; }
    }
}