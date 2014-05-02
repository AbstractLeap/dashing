using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using TopHat.Configuration.Mapper;
using TopHat.SqlWriter;

namespace TopHat.Configuration
{
    public interface IConfiguration
    {
        #region Conventions

        Conventions Conventions { get; set; }

        /// <summary>
        /// Specify the primary key identifier for your domain
        /// </summary>
        /// <param name="primaryKeyIdentifier"></param>
        /// <returns></returns>
        IConfiguration SetPrimaryKeyIdentifier(Func<PropertyInfo, bool> primaryKeyIdentifier);

        /// <summary>
        /// Indicates whether table names will be pluralised by default
        /// </summary>
        /// <param name="pluralise"></param>
        /// <returns></returns>
        IConfiguration SetPluraliseNamesByDefault(bool pluralise);

        /// <summary>
        /// Indicates whether table names will be pluralised by default
        /// </summary>
        /// <param name="pluraliseExpression"></param>
        /// <returns></returns>
        IConfiguration SetPluraliseNamesByDefault(Func<Type, bool> pluraliseExpression);

        /// <summary>
        /// Specify the default schema for all classes
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        IConfiguration SetDefaultSchema(string schema);

        /// <summary>
        /// Specify the schema for all classes
        /// </summary>
        /// <param name="schemaIdentifier"></param>
        /// <returns></returns>
        IConfiguration SetDefaultSchema(Func<Type, string> schemaIdentifier);

        /// <summary>
        /// Specify the default string length for string properties
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        IConfiguration SetDefaultStringLength(uint stringLength);

        /// <summary>
        /// Specify the default string length for string properties
        /// </summary>
        /// <param name="stringLengthExpression"></param>
        /// <returns></returns>
        IConfiguration SetDefaultStringLength(Func<PropertyInfo, uint> stringLengthExpression);

        /// <summary>
        /// Specify the default precision for decimal properties
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        IConfiguration SetDefaultDecimalPrecision(uint precision);

        /// <summary>
        /// Specify the default precision for decimal properties
        /// </summary>
        /// <param name="precisionExpression"></param>
        /// <returns></returns>
        IConfiguration SetDefaultDecimalPrecision(Func<PropertyInfo, uint> precisionExpression);

        /// <summary>
        /// Specify the default scale for decimal properties
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        IConfiguration SetDefaultDecimalScale(uint scale);

        /// <summary>
        /// Specify the default scale for decimal properties
        /// </summary>
        /// <param name="scaleExpression"></param>
        /// <returns></returns>
        IConfiguration SetDefaultDecimalScale(Func<PropertyInfo, uint> scaleExpression);

        #endregion Conventions

        #region Database

        /// <summary>
        /// The object to table mappings
        /// </summary>
        IDictionary<Type, IMap> Maps { get; }

        IDbConnection GetSqlConnection();

        ISqlWriter GetSqlWriter();

        #endregion Database

        #region Entities

        IConfiguration Add(Type type);

        IConfiguration Add<T>();

        IConfiguration Add(IEnumerable<Type> types);

        EntityMapper<T> Setup<T>();

        IConfiguration AddNamespaceFromAssemblyOf<T>(string nameSpace);

        #endregion Entities

        /// <summary>
        /// Run the configuration
        /// </summary>
        IConfiguration Configure();
    }
}