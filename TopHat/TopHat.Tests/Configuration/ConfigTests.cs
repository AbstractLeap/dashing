using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using Xunit;

namespace TopHat.Tests.Configuration
{
    public class ConfigTests
    {
        [Fact]
        public void DefaultConfigHasPluralisedTableNames()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();

            Assert.True(config.PluraliseNamesByDefault);
        }

        [Fact]
        public void DefaultConfigHas255StringLength()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();

            Assert.Equal(255, config.DefaultStringLength);
        }

        [Fact]
        public void DefaultConfigHasPrecision18()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();

            Assert.Equal(18, config.DefaultDecimalPrecision);
        }

        [Fact]
        public void DefaultConfigHasScale10()
        {
            var config = new DefaultConfiguration();
            var mapper = config.Configure();

            Assert.Equal(10, config.DefaultDecimalScale);
        }
    }
}