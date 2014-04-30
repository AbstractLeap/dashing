using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration.Mapper
{
    public class PropertyMapper<TEntity, TProperty>
    {
        private EntityMapper<TEntity> entityMapper;

        public PropertyMapper(EntityMapper<TEntity> entityMapper)
        {
            this.entityMapper = entityMapper;
        }

        public PropertyMapper<TEntity, TProperty> ColumnType(DbType dbType)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> ColumnType(string type)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> ColumnName(string name)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> DefaultExcluded()
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> Precision(int precision)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> Scale(int precision)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> Length(int length)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> IsDatabaseGenerated(bool isDBGenerated)
        {
            throw new NotImplementedException();
        }

        public PropertyMapper<TEntity, TProperty> Ignore()
        {
            throw new NotImplementedException();
        }
    }
}