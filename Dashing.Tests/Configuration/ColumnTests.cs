namespace Dashing.Tests.Configuration {
    using System;

    using Dashing.Configuration;
    using Dashing.Tests.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class ColumnTests {
        [Fact]
        public void FromThrowsIfColumnIsNull() {
            Assert.Throws<ArgumentNullException>(() => Column<Post>.From(null));
        }

        [Fact]
        public void FromThrowsIfColumnIsIncorrectType() {
            Assert.Throws<ArgumentException>(() => Column<string>.From(new Column<int>()));
        }

        [Fact]
        public void FromPopulatesAllProperties() {
            // assemble a populated column
            var column = new Column<string>().Populate() as IColumn;

            // act
            var genericColumn = Column<string>.From(column);

            // assert all properties are equal
            var columnType = column.GetType();
            var genericColumnType = genericColumn.GetType();
            foreach (var prop in columnType.GetProperties()) {
                Assert.Equal(prop.GetValue(column, null), genericColumnType.GetProperty(prop.Name).GetValue(genericColumn, null));
            }
        }
    }
}