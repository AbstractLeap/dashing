using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Tests.Configuration.DapperMapperGeneration {
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class CollectionTests {
        [Fact]
        public void SingleCollectionWorks() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var dict = new Dictionary<object, Post>();
            var func = (Func<Post, Comment, Post>)funcFac(dict);
            func(post1, comment1);
            func(post1, comment2);
            func(post2, comment3);
            Assert.Equal(1, dict[1].Comments.First().CommentId);
            Assert.Equal(2, dict[1].Comments.Last().CommentId);
            Assert.Equal(3, dict[2].Comments.First().CommentId);
        }

        private static Func<IDictionary<object, Post>, Delegate> GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(config.Engine, new Mock<IDbConnection>().Object).Fetch(p => p.Comments) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(new Mock<IGeneratedCodeManager>().Object);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree, false);
            return func;
        }

        class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}
