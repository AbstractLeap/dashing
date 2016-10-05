namespace Dashing.Tests.Configuration.AddNamespaceDomain {
    using Dashing.Configuration;

    [DoNotWeave]
    public class AddNamespaceConfiguration : BaseConfiguration {
        public AddNamespaceConfiguration() {
            this.AddNamespaceOf<Post>();
        }
    }

    public class Post {
        public int PostId { get; set; }

        public string Name { get; set; }
    }

    public class User {
        public string UserId { get; set; }

        public string Name { get; set; }
    }
}