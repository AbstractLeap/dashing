using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration.Mapper
{
    public class EntityMapper
    {
        private IConfiguration config;
        private Mapper mapper;

        public EntityMapper(IConfiguration config, Mapper mapper)
        {
            this.config = config;
            this.mapper = mapper;
        }

        public void Add<T>()
        {
            this.Add(typeof(T));
        }

        public void Add(Type type)
        {
        }
    }

    public class EntityMapper<T>
    {
        private IConfiguration configuration;
        private Mapper mapper;

        public EntityMapper(IConfiguration configuration, Mapper mapper)
        {
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public EntityMapper<T> Key<TResult>(Expression<Func<T, TResult>> keyExpression)
        {
            return this;
        }

        public EntityMapper<T> Schema(string schemaName)
        {
            this.configuration.Mapping.Maps[typeof(T)].Schema = schemaName;
            return this;
        }

        public EntityMapper<T> Table(string tableName)
        {
            this.configuration.Mapping.Maps[typeof(T)].Table = tableName;
            return this;
        }

        public PropertyMapper<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            return new PropertyMapper<T, TProperty>(this);
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