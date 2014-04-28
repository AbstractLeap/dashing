using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.Configuration
{
    public class EntityTests
    {
        [Fact]
        public void EntityAdded()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Add<Post>();
            Assert.NotNull(config.Mapping.Maps[typeof(Post)]);
        }

        [Fact]
        public void SpecifySingleKey()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().Key(u => u.Username);
            Assert.Equal("Username", config.Mapping.Maps[typeof(Post)].PrimaryKey);
        }

        [Fact]
        public void SpecifySchema()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().Schema("security");
            Assert.Equal("security", config.Mapping.Maps[typeof(Post)].Schema);
        }

        [Fact]
        public void SpecifyTable()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().Table("Identities");
            Assert.Equal("Identities", config.Mapping.Maps[typeof(Post)].Table);
        }
    }
}