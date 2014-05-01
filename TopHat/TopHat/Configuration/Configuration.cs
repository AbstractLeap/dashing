using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration.Mapper;
using TopHat.SqlWriter;

namespace TopHat.Configuration
{
    public abstract class Configuration : IConfiguration
    {
        private Mapper.Mapper mapper;

        public Configuration()
        {
            this.mapper = new Mapper.Mapper(this);
            this.Maps = new Dictionary<Type, IMap>();
            this.Conventions = new Conventions();
        }

        public Conventions Conventions { get; set; }

        public IDictionary<Type, IMap> Maps { get; private set; }

        public IDbConnection GetSqlConnection()
        {
            throw new NotImplementedException();
        }

        public ISqlWriter GetSqlWriter()
        {
            throw new NotImplementedException();
        }

        public abstract IConfiguration Configure();

        /// <summary>
        /// Adds a particular type in to the configuration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IConfiguration Add(Type type)
        {
            this.mapper.Add(type);
            return this;
        }

        /// <summary>
        /// Adds a particular type in to the configuration
        /// </summary>
        /// <returns></returns>
        public IConfiguration Add<T>()
        {
            this.mapper.Add<T>();
            return this;
        }

        /// <summary>
        /// Adds a list of types in to the configuration
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public IConfiguration Add(IEnumerable<Type> types)
        {
            this.mapper.Add(types);
            return this;
        }

        /// <summary>
        /// Enables setting up of a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityMapper<T> Setup<T>()
        {
            return this.mapper.Setup<T>();
        }

        /// <summary>
        /// Adds all of the classes within a namespace in to the configuration
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public IConfiguration AddNamespaceFromAssemblyOf<T>(string nameSpace)
        {
            return this.Add(typeof(T).Assembly.GetTypes().Where(t => t.Namespace == nameSpace));
        }
    }
}