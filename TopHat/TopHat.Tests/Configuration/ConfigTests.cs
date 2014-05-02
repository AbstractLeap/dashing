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
    public class ConfigTests
    {
        [Fact]
        public void DefaultConfigHasPluralisedTableNames()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();

            Assert.Equal("Posts", config.Maps[typeof(Post)].Table);
        }

        [Fact]
        public void DefaultConfigHas255StringLength()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();

            Assert.Equal<uint>(255, config.Maps[typeof(Post)].Columns["Content"].Length);
        }

        [Fact]
        public void DefaultConfigHasPrecision18()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();

            Assert.Equal<uint>(18, config.Maps[typeof(Post)].Columns["Rating"].Precision);
        }

        [Fact]
        public void DefaultConfigHasScale10()
        {
            var config = new DefaultConfiguration().Configure().Add<Post>();

            Assert.Equal<uint>(10, config.Maps[typeof(Post)].Columns["Rating"].Scale);
        }

        [Fact]
        public void DefaultPrimaryKeyIdentifierWords()
        {
            var config = new DefaultConfiguration().AddNamespaceFromAssemblyOf<Blog>(typeof(Blog).Namespace).Configure();
            Assert.Equal("BlogId", config.Maps[typeof(Blog)].PrimaryKey);
        }
    }
}