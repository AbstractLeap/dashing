using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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

        #region Conventions

        public Conventions Conventions { get; set; }

        public virtual IConfiguration SetPrimaryKeyIdentifier(Func<PropertyInfo, bool> primaryKeyIdentifier)
        {
            this.Conventions.PrimaryKeyIdentifier = primaryKeyIdentifier;
            return this;
        }

        public virtual IConfiguration SetPluraliseNamesByDefault(bool pluralise)
        {
            this.Conventions.PluraliseNamesByDefault = (t) => pluralise;
            return this;
        }

        public virtual IConfiguration SetPluraliseNamesByDefault(Func<Type, bool> pluraliseExpression)
        {
            this.Conventions.PluraliseNamesByDefault = pluraliseExpression;
            return this;
        }

        public virtual IConfiguration SetDefaultSchema(string schema)
        {
            this.Conventions.DefaultSchemaIdentifier = (t) => schema;
            return this;
        }

        public virtual IConfiguration SetDefaultSchema(Func<Type, string> schemaIdentifier)
        {
            this.Conventions.DefaultSchemaIdentifier = schemaIdentifier;
            return this;
        }

        public virtual IConfiguration SetDefaultStringLength(uint stringLength)
        {
            this.Conventions.DefaultStringLength = (p) => stringLength;
            return this;
        }

        public virtual IConfiguration SetDefaultStringLength(Func<PropertyInfo, UInt32> stringLengthExpression)
        {
            this.Conventions.DefaultStringLength = stringLengthExpression;
            return this;
        }

        public virtual IConfiguration SetDefaultDecimalPrecision(uint precision)
        {
            this.Conventions.DefaultDecimalPrecision = (p) => precision;
            return this;
        }

        public virtual IConfiguration SetDefaultDecimalPrecision(Func<PropertyInfo, UInt32> precisionExpression)
        {
            this.Conventions.DefaultDecimalPrecision = precisionExpression;
            return this;
        }

        public virtual IConfiguration SetDefaultDecimalScale(uint scale)
        {
            this.Conventions.DefaultDecimalScale = (p) => scale;
            return this;
        }

        public virtual IConfiguration SetDefaultDecimalScale(Func<PropertyInfo, UInt32> scaleExpression)
        {
            this.Conventions.DefaultDecimalScale = scaleExpression;
            return this;
        }

        #endregion Conventions

        #region Databases

        public IDictionary<Type, IMap> Maps { get; private set; }

        public virtual IDbConnection GetSqlConnection()
        {
            throw new NotImplementedException();
        }

        public virtual ISqlWriter GetSqlWriter()
        {
            throw new NotImplementedException();
        }

        #endregion Databases

        #region Entities

        /// <summary>
        /// Adds a particular type in to the configuration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IConfiguration Add(Type type)
        {
            this.mapper.Add(type);
            return this;
        }

        /// <summary>
        /// Adds a particular type in to the configuration
        /// </summary>
        /// <returns></returns>
        public virtual IConfiguration Add<T>()
        {
            this.mapper.Add<T>();
            return this;
        }

        /// <summary>
        /// Adds a list of types in to the configuration
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public virtual IConfiguration Add(IEnumerable<Type> types)
        {
            this.mapper.Add(types);
            return this;
        }

        /// <summary>
        /// Enables setting up of a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual EntityMapper<T> Setup<T>()
        {
            return this.mapper.Setup<T>();
        }

        /// <summary>
        /// Adds all of the classes within a namespace in to the configuration
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public virtual IConfiguration AddNamespaceFromAssemblyOf<T>(string nameSpace)
        {
            return this.Add(typeof(T).Assembly.GetTypes().Where(t => t.Namespace == nameSpace));
        }

        #endregion Entities

        public abstract IConfiguration Configure();
    }
}