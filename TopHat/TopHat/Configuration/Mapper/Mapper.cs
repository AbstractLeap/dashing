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

        private EntityMapper entityMapper;

        public Mapper(IConfiguration config)
        {
            this.config = config;
            this.entityMapper = new EntityMapper(config, this);
        }

        public void SetDefaultSchema(string schemaName)
        {
            this.config.DefaultSchema = schemaName;
        }

        public void Add(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.entityMapper.Add(type);
            }
        }

        public void Add<T>()
        {
            this.entityMapper.Add<T>();
        }

        public void Add(Type type)
        {
            this.entityMapper.Add(type);
        }

        public EntityMapper<T> Setup<T>()
        {
            return new EntityMapper<T>(this.config, this);
        }
    }
}