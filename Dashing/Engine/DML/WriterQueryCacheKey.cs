namespace Dashing.Engine.DML {
    using System;

    using Dashing.Configuration;

    public class WriterQueryCacheKey {
        public WriterQueryCacheKey(IConfiguration configuration, Type type) {
            this.Configuration = configuration;
            this.Type = type;
        }

        public IConfiguration Configuration { get; private set; }

        public Type Type { get; private set; }

        public override int GetHashCode() {
            unchecked {
                return this.Configuration.GetHashCode() * this.Type.GetHashCode();
            }
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherKey = obj as WriterQueryCacheKey;
            if (otherKey == null) {
                return false;
            }

            return Object.ReferenceEquals(this.Configuration, otherKey.Configuration) && this.Type == otherKey.Type;
        }
    }
}