namespace Dashing.Weaving.Sample.Domain {
    using System.Collections.Generic;

    public class Whopper {
        public int Id { get; set; }

        public string Name { get; set; }

        private IEnumerable<Duck> __ducks;

        private string filling;

        public IEnumerable<Duck> Ducks
        {
            get
            {
                return this.__ducks;
            }

            set
            {
                this.filling = "Burger";
                this.__ducks = value;
            }
        }

        public string GetFilling() {
            return this.filling;
        }
    }
}