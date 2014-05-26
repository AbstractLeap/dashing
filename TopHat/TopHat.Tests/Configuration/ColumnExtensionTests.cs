namespace TopHat.Tests.Configuration {
    using System;
    using System.Data;

    using TopHat.Configuration;

    using Xunit;

    public class ColumnExtensionsTests {
        private const string ExampleString = "foo";

        private const DbType ExampleDbType = DbType.String;

        private const byte ExampleByte = 128;

        private const ushort ExampleUshort = 1024;

        [Fact]
        public void NameThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.Name(ExampleString); });
        }

        [Fact]
        public void NameIsSet() {
            var column = MakeTarget();
            column.Name(ExampleString);
            Assert.Equal(ExampleString, column.Name);
        }

        [Fact]
        public void DbTypeThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.DbType(ExampleDbType); });
        }

        [Fact]
        public void DbTypeIsSet() {
            var column = MakeTarget();
            column.DbType(ExampleDbType);
            Assert.Equal(ExampleDbType, column.DbType);
        }

        [Fact]
        public void PrecisionThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.Precision(ExampleByte); });
        }

        [Fact]
        public void PrecisionIsSet() {
            var column = MakeTarget();
            column.Precision(ExampleByte);
            Assert.Equal(ExampleByte, column.Precision);
        }

        [Fact]
        public void ScaleThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.Scale(ExampleByte); });
        }

        [Fact]
        public void ScaleIsSet() {
            var column = MakeTarget();
            column.Scale(ExampleByte);
            Assert.Equal(ExampleByte, column.Scale);
        }

        [Fact]
        public void LengthThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.Length(ExampleUshort); });
        }

        [Fact]
        public void LengthIsSet() {
            var column = MakeTarget();
            column.Length(ExampleUshort);
            Assert.Equal(ExampleUshort, column.Length);
        }

        [Fact]
        public void ExcludeByDefaultThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.ExcludeByDefault(); });
        }

        [Fact]
        public void ExcludeByDefaultSetsFlag() {
            var column = MakeTarget();
            column.ExcludeByDefault();
            Assert.Equal(true, column.IsExcludedByDefault);
        }

        [Fact]
        public void DontExcludeByDefaultThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.DontExcludeByDefault(); });
        }

        [Fact]
        public void DontExcludeByDefaultSetsFlag() {
            var column = MakeTarget();
            column.DontExcludeByDefault();
            Assert.Equal(false, column.IsExcludedByDefault);
        }

        [Fact]
        public void IgnoreThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.Ignore(); });
        }

        [Fact]
        public void IgnoreSetsFlag() {
            var column = MakeTarget();
            column.Ignore();
            Assert.Equal(true, column.IsIgnored);
        }

        [Fact]
        public void DontIgnoreThrowsIfColumnIsNull() {
            var column = MakeNullTarget();
            Assert.Throws<ArgumentNullException>(() => { column.DontIgnore(); });
        }

        [Fact]
        public void DontIgnoreSetsFlag() {
            var column = MakeTarget();
            column.DontIgnore();
            Assert.Equal(false, column.IsIgnored);
        }

        private static Column<string> MakeNullTarget() {
            return default(Column<string>);
        }

        private static Column<string> MakeTarget() {
            return new Column<string>();
        }
    }
}