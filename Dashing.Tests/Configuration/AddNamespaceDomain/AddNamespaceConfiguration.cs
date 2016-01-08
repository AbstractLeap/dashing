namespace Dashing.Tests.Configuration.AddNamespaceDomain {
    using System.Configuration;

    using Dashing.Configuration;

    [DoNotWeave]
    public class AddNamespaceConfiguration : DefaultConfiguration {
        public AddNamespaceConfiguration()
            : base(new ConnectionStringSettings("Default", "", "System.Data.SqlClient")) {
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