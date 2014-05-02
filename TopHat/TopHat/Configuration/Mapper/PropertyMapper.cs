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
        private IConfiguration configuration;
        private EntityMapper<TEntity> entityMapper;
        private IMap map;
        private string propertyName;

        public PropertyMapper(IConfiguration configuration, EntityMapper<TEntity> entityMapper, string propertyName)
        {
            // TODO: Complete member initialization
            this.entityMapper = entityMapper;
            this.propertyName = propertyName;
            this.configuration = configuration;
            this.map = this.configuration.Maps[typeof(TEntity)];
        }

        public PropertyMapper<TEntity, TProperty> ColumnType(DbType dbType)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].ColumnType = dbType;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> ColumnType(string type)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].ColumnTypeString = type;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> ColumnName(string name)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].ColumnName = name;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> DefaultExcluded()
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].IncludeByDefault = false;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> Precision(uint precision)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].Precision = precision;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> Scale(uint scale)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].Scale = scale;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> Length(uint length)
        {
            CheckColumnExists();
            this.map.Columns[this.propertyName].Length = length;
            return this;
        }

        public PropertyMapper<TEntity, TProperty> Ignore()
        {
            // We won't use CheckColumnExists here as frankly it doesn't matter
            if (this.map.Columns.ContainsKey(this.propertyName))
            {
                this.map.Columns.Remove(this.propertyName);
            }

            return this;
        }

        private void CheckColumnExists()
        {
            if (!this.map.Columns.ContainsKey(this.propertyName))
            {
                throw new ArgumentException("The property " + this.propertyName + " does not exist in the map");
            }
        }
    }
}