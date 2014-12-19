namespace Dashing.Tools.Tests {
    using System;
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Tools.Tests.TestDomain;

    public class CustomConfig : DefaultConfiguration, ISeeder {
        public CustomConfig()
            : base(new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=dashingtest;Integrated Security=True", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
        }

        public void Seed(ISession session) {
            var blog = new Blog { Title = "My Blog", CreateDate = DateTime.Now };
            session.InsertOrUpdate(blog);

            var user = new User { Username = "Mark", EmailAddress = "maj@113.com" };
            session.InsertOrUpdate(user);
        }
    }
}