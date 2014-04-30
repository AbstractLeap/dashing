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

        [Fact]
        public void IncludeByDefault()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Add<Post>();
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "PostId" && c.IncludeByDefault) == 1);
        }

        [Fact]
        public void ExcludeWorks()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.Content).DefaultExcluded();
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "Content" && !c.IncludeByDefault) == 1);
        }

        [Fact]
        public void SetPrecisionWorks()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.Rating).Precision(10);
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "Rating" && c.Precision == 10) == 1);
        }

        [Fact]
        public void SetScaleWorks()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.Rating).Scale(10);
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "Rating" && c.Scale == 10) == 1);
        }

        [Fact]
        public void SetLengthWorks()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.Content).Length(4000);
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "Content" && c.Length == 4000) == 1);
        }

        [Fact]
        public void IsDBGeneratedWorks()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.PostId).Length(4000);
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "Content" && c.Length == 4000) == 1);
        }

        [Fact]
        public void IgnorePropertyNotInMapping()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();
            mapper.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            Assert.True(config.Mapping.Maps[typeof(Post)].Columns.Count(c => c.PropertyName == "DoNotMap") == 0);
        }
    }
}