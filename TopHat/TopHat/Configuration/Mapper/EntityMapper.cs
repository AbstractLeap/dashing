using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TopHat.Extensions;

namespace TopHat.Configuration.Mapper
{
    public class EntityMapper<T>
    {
        private IConfiguration configuration;
        private Mapper mapper;
        private Type type;

        private PluralizationService pluraliser = PluralizationService.CreateService(new System.Globalization.CultureInfo("en-GB"));

        public EntityMapper(IConfiguration configuration, Mapper mapper)
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.type = typeof(T);

            this.MapIfNecessary();
        }

        private void MapIfNecessary()
        {
            if (!this.configuration.Maps.ContainsKey(this.type))
            {
                var map = new Map<T>();

                // figure out the table name
                var tableName = this.type.Name;
                if (this.configuration.Conventions.PluraliseNamesByDefault)
                {
                    tableName = pluraliser.Pluralize(tableName);
                }

                map.Table = tableName;

                // figure out the schema
                var schema = string.Empty;
                if (this.configuration.Conventions.DefaultSchema != null)
                {
                    schema = this.configuration.Conventions.DefaultSchema;
                }

                map.Schema = schema;

                // do type
                map.Type = type;

                // load in the columns
                var extraValueTypes = new Type[] { typeof(String), typeof(byte[]) };
                foreach (var property in this.type.GetProperties())
                {
                    var column = new Column { ColumnName = property.Name, PropertyName = property.Name, PropertyType = property.PropertyType };

                    // check to see if primary key
                    if (this.configuration.Conventions.PrimaryKeyIdentifier(property))
                    {
                        map.PrimaryKey = column.PropertyName;
                    }

                    // need to determine the type of the column
                    // and then treat accordingly
                    if (!property.PropertyType.IsValueType && !extraValueTypes.Contains(property.PropertyType))
                    {
                        if (property.PropertyType.IsCollection())
                        {
                            // assume to be OneToMany
                            column.Relationship = RelationshipType.OneToMany;
                        }
                        else
                        {
                            column.Relationship = RelationshipType.ManyToOne;
                            column.ColumnName = property.Name + "Id";

                            // TODO resolve column type of related primary key - be careful with infinite loops!
                        }
                    }
                    else
                    {
                        column.Relationship = RelationshipType.None;
                        column.ColumnType = property.PropertyType.GetDBType();

                        // check particular types for defaults
                        if (column.ColumnType == System.Data.DbType.Decimal)
                        {
                            column.Precision = this.configuration.Conventions.DefaultDecimalPrecision;
                            column.Scale = this.configuration.Conventions.DefaultDecimalScale;
                        }
                        else if (column.ColumnType == System.Data.DbType.String)
                        {
                            column.Length = this.configuration.Conventions.DefaultStringLength;
                        }

                        // TODO Add nullable column types
                    }

                    map.Columns.Add(column.PropertyName, column);
                }

                this.configuration.Maps.Add(this.type, map);
            }
        }

        public EntityMapper<T> Key<TResult>(Expression<Func<T, TResult>> keyExpression)
        {
            return this;
        }

        public EntityMapper<T> Schema(string schemaName)
        {
            this.configuration.Maps[typeof(T)].Schema = schemaName;
            return this;
        }

        public EntityMapper<T> Table(string tableName)
        {
            this.configuration.Maps[typeof(T)].Table = tableName;
            return this;
        }

        public PropertyMapper<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("propertyExpression must be a valid MemberExpression");
            }

            var propertyMapper = new PropertyMapper<T, TProperty>(this.configuration, this, memberExpression.Member.Name);
            return propertyMapper;
        }

        public EntityMapper<T> PrimaryKeyDatabaseGenerated(bool databaseGenerated)
        {
            throw new NotImplementedException();
        }

        public EntityMapper<T> Index<TProperty>(Expression<Func<T, TProperty>> newExpression)
        {
            throw new NotImplementedException();
        }
    }
}