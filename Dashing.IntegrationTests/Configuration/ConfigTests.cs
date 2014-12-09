using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.Configuration {
    using Dashing.Configuration;
    using Dashing.IntegrationTests.Configuration.Domain;

    using Xunit;

    public class ConfigTests {
        [Fact]
        public void ConfigDoesNotThrow() {
            Assert.DoesNotThrow(() => new TestConfig());
        }

        [Fact]
        public void OneToOneLeftIsOneToOne() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.OneToOne, config.GetMap<OneToOneLeft>().Columns["Right"].Relationship);
        }

        [Fact]
        public void OneToOneRightIsOneToOne() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.OneToOne, config.GetMap<OneToOneRight>().Columns["Left"].Relationship);
        }

        [Fact]
        public void OneToOneLeftHasGoodOppositeColumn() {
            var config = new TestConfig();
            Assert.Equal("Left", config.GetMap<OneToOneLeft>().Columns["Right"].OppositeColumn.Name);
        }

        [Fact]
        public void PairReferencesIsOneToOneSelfReference() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.OneToOne, config.GetMap<Pair>().Columns["References"].Relationship);
        }

        [Fact]
        public void PairReferencedByIsOneToOneSelfReference() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.OneToOne, config.GetMap<Pair>().Columns["ReferencedBy"].Relationship);
        }

        [Fact]
        public void PairReferencesHasGoodOppositeColumn() {
            var config = new TestConfig();
            Assert.Equal("ReferencedBy", config.GetMap<Pair>().Columns["References"].OppositeColumn.Name);
        }

        [Fact]
        public void PairReferencedByHasGoodOppositeColumn() {
            var config = new TestConfig();
            Assert.Equal("References", config.GetMap<Pair>().Columns["ReferencedBy"].OppositeColumn.Name);
        }

        [Fact]
        public void CategorySelfReferenceIsOneToMany() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.OneToMany, config.GetMap<Category>().Columns["Children"].Relationship);
        }

        [Fact]
        public void CategorySelfReferenceIsManyToOne() {
            var config = new TestConfig();
            Assert.Equal(RelationshipType.ManyToOne, config.GetMap<Category>().Columns["Parent"].Relationship);
        }
    }
}
