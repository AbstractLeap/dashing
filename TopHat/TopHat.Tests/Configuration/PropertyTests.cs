using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using Assert = Xunit.Assert;

namespace TopHat.Tests.Configuration
{
    public class PropertyTests
    {
        [Fact]
        public void SpecifyDbType()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.PostId).ColumnType(DbType.Int16);
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "PostId" && c.ColumnType == DbType.Int16) == 1);
        }

        [Fact]
        public void SpecifyColumnTypeString()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.PostId).ColumnType("TEXT");
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "PostId" && c.ColumnTypeString == "TEXT") == 1);
        }

        [Fact]
        public void SpecifyColumnName()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.PostId).ColumnName("Id");
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "PostId" && c.ColumnName == "Id") == 1);
        }

        [Fact]
        public void IncludeByDefault()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "PostId" && c.IncludeByDefault) == 1);
        }

        [Fact]
        public void ExcludeWorks()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.Content).DefaultExcluded();
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Content" && !c.IncludeByDefault) == 1);
        }

        [Fact]
        public void SetPrecisionWorks()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.Rating).Precision(10);
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Rating" && c.Precision == 10) == 1);
        }

        [Fact]
        public void SetScaleWorks()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.Rating).Scale(10);
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Rating" && c.Scale == 10) == 1);
        }

        [Fact]
        public void SetLengthWorks()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.Content).Length(4000);
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Content" && c.Length == 4000) == 1);
        }

        [Fact]
        public void IsDBGeneratedWorks()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.PostId).Length(4000);
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Content" && c.Length == 4000) == 1);
        }

        [Fact]
        public void IgnorePropertyNotInMapping()
        {
            var config = new DefaultConfiguration().Configure();
            config.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "DoNotMap") == 0);
        }

        [Fact]
        public void DefaultStringLengthCallsLambda()
        {
            // instrumentation
            var propertyInfos = new List<PropertyInfo>();
            const int defaultLength = 255;

            // assemble
            var config = new DefaultConfiguration().Configure();
            config.Conventions.DefaultStringLength = (p) =>
            {
                propertyInfos.Add(p);
                return defaultLength;
            };

            // act
            config.Add<Post>();

            // assert
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Content" && c.Length == defaultLength) == 1);
            CollectionAssert.Contains(propertyInfos, typeof(Post).GetProperty("Content"));
        }

        [Fact]
        public void DefaultDecimalPrecisionCallsLambda()
        {
            // instrumentation
            var propertyInfos = new List<PropertyInfo>();
            const int defaultPrecision = 6;

            // assemble
            var config = new DefaultConfiguration().Configure();
            config.Conventions.DefaultDecimalPrecision = (p) =>
            {
                propertyInfos.Add(p);
                return defaultPrecision;
            };

            // act
            config.Add<Post>();

            // assert
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Rating" && c.Precision == defaultPrecision) == 1);
            CollectionAssert.Contains(propertyInfos, typeof(Post).GetProperty("Rating"));
        }

        [Fact]
        public void DefaultDecimalScaleCallsLambda()
        {
            // instrumentation
            var propertyInfos = new List<PropertyInfo>();
            const int defaultScale = 5;

            // assemble
            var config = new DefaultConfiguration().Configure();
            config.Conventions.DefaultDecimalScale = (p) =>
            {
                propertyInfos.Add(p);
                return defaultScale;
            };

            // act
            config.Add<Post>();

            // assert
            Assert.True(config.Maps[typeof(Post)].Columns.Select(k => k.Value).Count(c => c.PropertyName == "Rating" && c.Scale == defaultScale) == 1);
            CollectionAssert.Contains(propertyInfos, typeof(Post).GetProperty("Rating"));
            
        }

    }
}