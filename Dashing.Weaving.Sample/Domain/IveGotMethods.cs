namespace Dashing.Weaving.Sample.Domain {
    public class IveGotMethods {
        public int Id { get; set; }

        public string Name { get; set; }

        public override int GetHashCode() {
            return 42;
        }

        public override bool Equals(object obj) {
            return obj == null;
        }
    }
}