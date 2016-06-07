namespace Dashing.Weaving.Sample.Domain {
    public class Starship {
        private bool __foo;

        private bool __bar;

        public int Id { get; set; }

        public bool Foo
        {
            get
            {
                return this.__foo;
            }

            set
            {
                this.__bar = true;
                this.__foo = value;
            }
        }

        public bool GetBar() {
            return this.__bar;
        }
    }
}