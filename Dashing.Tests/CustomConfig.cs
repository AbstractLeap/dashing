namespace Dashing.Tools.Tests {
    using System;

    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.Constructor;

    public class CustomConfig : BaseConfiguration, ISeeder {
        public CustomConfig() {
            this.AddNamespaceOf<Post>();
        }

        public void Seed(ISession session) {
            var blog = new Blog { Title = "My Blog", CreateDate = DateTime.Now };
            session.InsertOrUpdate(blog);

            var user = new User { Username = "Mark", EmailAddress = "maj@113.com" };
            session.InsertOrUpdate(user);
        }
    }

    public class ClassWithConstructorConfig : BaseConfiguration {
        public ClassWithConstructorConfig() {
            this.Add<ClassWithConstructor>();
        }
    }
}