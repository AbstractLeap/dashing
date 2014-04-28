using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.Configuration
{
    public class PropertyTests
    {
        [Fact]
        public void SpecifyDbType()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.PostId).ColumnType(DbType.Int16);
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "PostId" && c.ColumnType == DbType.Int16) == 1);
        }

        [Fact]
        public void SpecifyColumnTypeString()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.PostId).ColumnType("TEXT");
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "PostId" && c.ColumnTypeString == "TEXT") == 1);
        }

        [Fact]
        public void SpecifyColumnName()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.PostId).ColumnName("TEXT");
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "PostId" && c.ColumnName == "Id") == 1);
        }
    }
}