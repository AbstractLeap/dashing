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
        public void CallingSetupInvokesAddIfNecessary()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().Key(u => u.Username);
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
        public void PKDbGenerated()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().PrimaryKeyDatabaseGenerated(true);
            Assert.True(config.Mapping.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
        }

        [Fact]
        public void PKDbGeneratedFalse()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<User>().PrimaryKeyDatabaseGenerated(false);
            Assert.False(config.Mapping.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
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

        [Fact]
        public void DefaultConfigurationPKDbGenerated()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Add<Post>();
            Assert.False(config.Mapping.Maps[typeof(Post)].IsPrimaryKeyDatabaseGenerated);
        }

        [Fact]
        public void DefaultConfigurationFKIndexesGenerated()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            Assert.True(config.GenerateIndexesOnForeignKeysByDefault);
        }

        [Fact]
        public void IndexSetCorrectlySingleProperty()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Index(p => p.Title);
            Assert.True(config.Mapping.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.First() == "Title") == 1);
        }

        [Fact]
        public void IndexSetCorrectlyMultipleProperties()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Index(p => new { p.PostId, p.Title });
            Assert.True(config.Mapping.Maps[typeof(Post)].Indexes.Count(l => l.Count == 1 && l.Contains("Title") && l.Contains("PostId")) == 1);
        }
    }
}