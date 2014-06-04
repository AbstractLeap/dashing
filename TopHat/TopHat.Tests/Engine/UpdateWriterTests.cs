using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Tests.Engine
{
    using System.Diagnostics;

    using TopHat.CodeGeneration;
    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Tests.CodeGeneration.Fixtures;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class UpdateWriterTests : IUseFixture<GenerateCodeFixture>
    {
        private IGeneratedCodeManager codeManager;

        public void SetFixture(GenerateCodeFixture data)
        {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void UpdateSinglePropertyWorks() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.PostId = 1;
            post.Title = "Boo";
            this.codeManager.TrackInstance(post);
            post.Title = "New Boo";
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var query = new UpdateEntityQuery<Post>(post);
            Debug.Write(updateWriter.GenerateSql(query).Sql);
        }

        private static IConfiguration MakeConfig(bool withIgnore = false)
        {
            if (withIgnore)
            {
                return new CustomConfigWithIgnore();
            }

            return new CustomConfig();
        }

        private class CustomConfig : DefaultConfiguration
        {
            public CustomConfig()
                : base(new SqlServerEngine(), string.Empty)
            {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : DefaultConfiguration
        {
            public CustomConfigWithIgnore()
                : base(new SqlServerEngine(), string.Empty)
            {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}
