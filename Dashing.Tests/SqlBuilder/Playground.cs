﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dashing.Tests.SqlBuilder
{
    public class Playground
    {
        [Fact]
        public void Test()
        {
            var config = new TestConfig();
            var database = new InMemoryDatabase(config);
            var sqlBuilder = new Dashing.SqlBuilder.SqlBuilder(database.BeginSession());
            var query = sqlBuilder.From<TestDomain.Post>()
                      .InnerJoin<TestDomain.User>((post, user) => post.Author == user)
                      .Where((post, user) => post.Blog.BlogId == 1)
                      .Select((post, user) => post.Title);

        }

    }
}