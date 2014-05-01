using System;
using System.Collections.Generic;
using System.Data;
using TopHat.Configuration.Mapper;
using TopHat.SqlWriter;

namespace TopHat.Configuration
{
    public interface IConfiguration
    {
        Conventions Conventions { get; set; }

        /// <summary>
        /// The object to table mappings
        /// </summary>
        IDictionary<Type, IMap> Maps { get; }

        IDbConnection GetSqlConnection();

        ISqlWriter GetSqlWriter();

        /// <summary>
        /// Run the configuration
        /// </summary>
        IConfiguration Configure();

        IConfiguration Add(Type type);

        IConfiguration Add<T>();

        IConfiguration Add(IEnumerable<Type> types);

        EntityMapper<T> Setup<T>();

        IConfiguration AddNamespaceFromAssemblyOf<T>(string nameSpace);
    }
}