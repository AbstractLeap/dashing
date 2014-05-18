namespace TopHat.Tests.Configuration {
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq.Expressions;

  using Moq;

  using global::TopHat.Configuration;

  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class DefaultMapperTests {
    private readonly Mock<IConvention> mockConvention = new Mock<IConvention>();

    private const string ExampleString = "foo";

    private const byte ExampleByte = 128;

    private const ushort ExampleUshort = 1024;

    [Fact]
    public void ConstructorThrowsOnNullConvention() {
      Assert.Throws<ArgumentNullException>(() => new DefaultMapper(null));
    }

    [Fact]
    public void DelegatesTableNameToConvention() {
      this.mockConvention.Setup(m => m.TableFor(typeof(User))).Returns(ExampleString).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      Assert.Equal(ExampleString, map.Table);
      this.mockConvention.Verify();
    }

    [Fact]
    public void DelegatesSchemaNameToConvention() {
      this.mockConvention.Setup(m => m.SchemaFor(typeof(User))).Returns(ExampleString).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      Assert.Equal(ExampleString, map.Schema);
      this.mockConvention.Verify();
    }

    [Fact]
    public void DelegatesPrimaryKeyNameToConvention() {
      this.mockConvention.Setup(m => m.PrimaryKeyOf(typeof(User))).Returns(ExampleString).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      Assert.Equal(ExampleString, map.PrimaryKey);
      this.mockConvention.Verify();
    }

    [Fact]
    public void StringLengthDelegatesToConvention() {
      this.mockConvention.Setup(m => m.StringLengthFor(typeof(User), "Username")).Returns(ExampleUshort).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      var property = map.Property(u => u.Username);
      Assert.NotNull(property);
      Assert.Equal(ExampleUshort, property.Length);
      this.mockConvention.Verify();
    }

    [Fact]
    public void DecimalPrecisionDelegatesToConvention() {
      this.mockConvention.Setup(m => m.DecimalPrecisionFor(typeof(User), "HeightInMeters")).Returns(ExampleByte).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      var property = map.Property(u => u.HeightInMeters);
      Assert.NotNull(property);
      Assert.Equal(ExampleByte, property.Precision);
      this.mockConvention.Verify();
    }

    [Fact]
    public void DecimalScaleDelegatesToConvention() {
      this.mockConvention.Setup(m => m.DecimalScaleFor(typeof(User), "HeightInMeters")).Returns(ExampleByte).Verifiable();
      var target = this.MakeTarget();

      var map = target.MapFor<User>();

      Assert.NotNull(map);
      var property = map.Property(u => u.HeightInMeters);
      Assert.NotNull(property);
      Assert.Equal(ExampleByte, property.Scale);
      this.mockConvention.Verify();
    }

    [Fact]
    public void ColumnTypeIsSet() {
      var property = this.MapAndGetProperty<User, string>(u => u.Username);
      Assert.Equal(typeof(string), property.Type);
    }

    [Fact]
    public void NonEntityColumnNameIsSet() {
      var property = this.MapAndGetProperty<User, string>(u => u.Username);
      Assert.Equal("Username", property.Name);
    }

    [Fact]
    public void NonEntityColumnDbTypeIsSet() {
      var property = this.MapAndGetProperty<User, string>(u => u.Username);
      Assert.Equal(DbType.String, property.DbType);
    }

    [Fact]
    public void NonEntityColumnRelationshipIsNone() {
      var property = this.MapAndGetProperty<User, string>(u => u.Username);
      Assert.Equal(RelationshipType.None, property.Relationship);
    }

    [Fact]
    public void EntityColumnNameIsSet() {
      var property = this.MapAndGetProperty<Post, User>(p => p.Author);
      Assert.Equal("AuthorId", property.Name);
    }

    [Fact]
    public void EntityColumnDbTypeIsSet() {
      var property = this.MapAndGetProperty<Post, User>(p => p.Author);
      Assert.Equal(DbType.Int32, property.DbType);
    }

    [Fact]
    public void EntityColumnRelationshipIsManyToOne() {
      var property = this.MapAndGetProperty<Post, User>(u => u.Author);
      Assert.Equal(RelationshipType.ManyToOne, property.Relationship);
    }

    [Fact(Skip = "Check with Mark what he expects here")]
    public void EntityCollectionIsIgnored() {
      var property = this.MapAndGetProperty<Post, ICollection<Comment>>(p => p.Comments);
      Assert.Equal(true, property.Ignore);
    }

    [Fact]
    public void EntityCollectionColumnRelationshipIsOneToMany() {
      var property = this.MapAndGetProperty<Post, User>(u => u.Author);
      Assert.Equal(RelationshipType.OneToMany, property.Relationship);
    }

    private DefaultMapper MakeTarget() {
      return new DefaultMapper(this.mockConvention.Object);
    }

    private Column<TProperty> MapAndGetProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression) {
      var target = this.MakeTarget();
      var map = target.MapFor<T>();
      Assert.NotNull(map);
      var property = map.Property(propertyExpression);
      Assert.NotNull(property);
      return property;
    }
  }
}