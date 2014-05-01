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
            var config = new DefaultConfiguration().Configure().Add<Post>();
            Assert.NotNull(config.Maps[typeof(Post)]);
        }

        [Fact]
        public void CallingSetupInvokesAddIfNecessary()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().Key(u => u.Username);
            Assert.NotNull(config.Maps[typeof(Post)]);
        }

        [Fact]
        public void SpecifySingleKey()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().Key(u => u.Username);
            Assert.Equal("Username", config.Maps[typeof(Post)].PrimaryKey);
        }

        [Fact]
        public void PKDbGenerated()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().PrimaryKeyDatabaseGenerated(true);
            Assert.True(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
        }

        [Fact]
        public void PKDbGeneratedFalse()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().PrimaryKeyDatabaseGenerated(false);
            Assert.False(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
        }

        [Fact]
        public void SpecifySchema()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().Schema("security");
            Assert.Equal("security", config.Maps[typeof(Post)].Schema);
        }

        [Fact]
        public void SpecifyTable()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<User>().Table("Identities");
            Assert.Equal("Identities", config.Maps[typeof(Post)].Table);
        }

        [Fact]
        public void DefaultConfigurationPKDbGenerated()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();
            Assert.False(config.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
        }

        [Fact]
        public void DefaultConfigurationFKIndexesGenerated()
        {
            var config = new DefaultConfiguration().Configure();
            Assert.True(config.GenerateIndexesOnForeignKeysByDefault);
        }

        [Fact]
        public void IndexSetCorrectlySingleProperty()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Index(p => p.Title);
            Assert.True(config.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.First() == "Title") == 1);
        }

        [Fact]
        public void IndexSetCorrectlyMultipleProperties()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Index(p => new { p.PostId, p.Title });
            Assert.True(config.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.Contains("Title") && l.Contains("PostId")) == 1);
        }
    }
}