using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration.Mapper
{
    public class Mapper
    {
        private IConfiguration config;

        public Mapper(IConfiguration config)
        {
            this.config = config;
        }

        public void SetDefaultSchema(string schemaName)
        {
            this.config.DefaultSchema = schemaName;
        }

        public void Add(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.Add(type);
            }
        }

        public void Add<T>()
        {
            this.Add(typeof(T));
        }

        public void Add(Type type)
        {
            // let's create an instance of an EntityMapper for this type
            // this will create the mapping
            var mapperType = typeof(EntityMapper<>).MakeGenericType(type);
            Activator.CreateInstance(mapperType, new object[] { this.config, this });
        }

        public EntityMapper<T> Setup<T>()
        {
            return new EntityMapper<T>(this.config, this);
        }
    }
}