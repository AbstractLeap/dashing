namespace TopHat.Tests.Configuration {
    using TopHat.Configuration;
    using TopHat.Tests.Extensions;

    using Xunit;

    public class MapTests {
        [Fact]
        public void ColumnsCollectionIsInitialised() {
            var target = new Map<string>();
            Assert.NotNull(target.Columns);
        }

        [Fact]
        public void FromPopulatesAllProperties() {
            // assemble a populated column
            var imap = new Map<string>().Populate(new Column<string>() as IColumn) as IMap;

            // act
            var map = Map<string>.From(imap);

            // assert all properties are equal
            var columnType = imap.GetType();
            var genericColumnType = map.GetType();
            foreach (var prop in columnType.GetProperties()) {
                Assert.Equal(prop.GetValue(imap, null), genericColumnType.GetProperty(prop.Name).GetValue(map, null));
            }
        }
    }
}