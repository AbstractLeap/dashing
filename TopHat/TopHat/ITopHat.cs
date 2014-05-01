using System;
using System.Data;

using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat
{
    public interface ITopHat : IDisposable
    {
        #region Properties

        /// <summary>
        /// The IDbConnection object associated with this instance of TopHat
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// The IDbTransaction object associated with this instance of TopHat
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// The Mapping object corresponding to this instance of TopHat
        /// </summary>
        IConfiguration Configuration { get; }

        #endregion Properties

        #region Transactions

        /// <summary>
        /// Indicate that the transactional worked completely successfully and therefore commit the transaction.
        /// In order to rollback simply refrain from calling this method and dispose will rollback.
        /// </summary>
        void Complete();

        #endregion Transactions

        #region CUD

        /// <summary>
        /// Inserts a new entity in to the database
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">The entity to insert</param>
        void Insert<T>(T entity);

        /// <summary>
        /// Updates the entity in the database
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">The entity to update</param>
        void Update<T>(T entity);

        /// <summary>
        /// Enables updating entities directly in the database based on a where clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IWhereExecute<T> Update<T>();

        /// <summary>
        /// Deletes the entity from the database
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">The entity to delete</param>
        void Delete<T>(T entity);

        /// <summary>
        /// Deletes an entity from the database based on the primary key
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="id">The integer primary key of the entity to delete</param>
        /// <remarks>Will throw an exception if the entity has a non-integer primary key</remarks>
        void Delete<T>(int id);

        /// <summary>
        /// Enables deletion of multiple entities in the database without having to first fetch the entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IWhereExecute<T> Delete<T>();

        #endregion CUD

        #region Querying

        /// <summary>
        /// Construct a query against the database
        /// </summary>
        /// <typeparam name="T">Type of entity to select</typeparam>
        /// <returns></returns>
        ISelect<T> Query<T>();

        /// <summary>
        /// Construct a query against the database and add change tracking to any returned objects
        /// </summary>
        /// <typeparam name="T">Type of entities to return</typeparam>
        /// <returns></returns>
        ISelect<T> QueryTracked<T>();

        #endregion Querying
    }
}