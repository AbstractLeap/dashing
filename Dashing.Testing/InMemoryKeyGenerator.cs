namespace Dashing.Testing {
    using System;

    public class InMemoryKeyGenerator<TPrimaryKey> {
        private long latest;

        public object GetNextPrimaryKey() {
            if (typeof(TPrimaryKey) == typeof(Guid)) {
                return Guid.NewGuid();
            }
            if (typeof(TPrimaryKey) == typeof(int)) {
                this.latest++;
                return (int)this.latest;
            }
            if (typeof(TPrimaryKey) == typeof(short)) {
                this.latest++;
                return (short)this.latest;
            }
            if (typeof(TPrimaryKey) == typeof(long)) {
                this.latest++;
                return this.latest;
            }
            if (typeof(TPrimaryKey) == typeof(byte)) {
                this.latest++;
                return (byte)this.latest;
            }
            throw new Exception("Unknown Primary Key Type");
        }
    }
}