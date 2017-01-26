namespace Dashing.Weaving.Tests {
    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;

    using Xunit;

    [Collection("Weaving Tests")]
    public class SetLoggerTests {
        [Fact]
        public void EntityIsSetLogger() {
            var bar = new Bar();
            Assert.IsAssignableFrom<ISetLogger>(bar);
        }

        [Fact]
        public void NewEntityHasNoSetProperties() {
            var bar = new Bar();
            Assert.Empty(((ISetLogger)bar).GetSetProperties());
        }

        [Fact]
        public void SetPropertiesMakesSet() {
            var bar = new Bar();
            ((ISetLogger)bar).EnableSetLogging();
            bar.Name = "Foo";
            Assert.Equal(new[] { "Name" }, ((ISetLogger)bar).GetSetProperties());
        }

        [Fact]
        public void SetPkNotSet() {
            var bar = new Bar();
            ((ISetLogger)bar).EnableSetLogging();
            bar.Id = 1;
            Assert.Empty(((ISetLogger)bar).GetSetProperties());
        }

        [Fact]
        public void NotEnablingLoggingDoesNotMakeSet() {
            var bar = new Bar();
            bar.Name = "Foo";
            Assert.Empty(((ISetLogger)bar).GetSetProperties());
        }

        [Fact]
        public void ConstructedSetLoggerNotEnabled() {
            var bar = new Bar();
            Assert.False(((ISetLogger)bar).IsSetLoggingEnabled());
        }

        [Fact]
        public void DisableSetLoggingClears() {
            var bar = new Bar();
            ((ISetLogger)bar).EnableSetLogging();
            bar.Name = "foo";
            ((ISetLogger)bar).DisableSetLogging();
            Assert.Empty(((ISetLogger)bar).GetSetProperties());
        }
    }
}