﻿namespace Dashing.Configuration {
    using System;

    /// <summary>
    ///     The Convention interface.
    /// </summary>
    public interface IConvention {
        /// <summary>
        ///     Identifies the table name for an entity type
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string TableFor(Type entity);

        /// <summary>
        ///     Identifies the schema name for an entity type (if specified, else null for the default schema)
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string SchemaFor(Type entity);

        /// <summary>
        ///     Identifies the primary key name for a given entity
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The property name of the primary key.
        /// </returns>
        bool IsPrimaryKeyFor(Type entity, string propertyName);

        /// <summary>
        ///     Identifies whether the primary key is auto-generated for a given entity
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        bool IsPrimaryKeyAutoGenerated(Type entity);

        /// <summary>
        ///     Specifies the string length for a property
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property Name.
        /// </param>
        /// <returns>
        ///     The <see cref="ushort" />.
        /// </returns>
        ushort StringLengthFor(Type entity, string propertyName);

        /// <summary>
        ///     Specifies the decimal precision for a property
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property Name.
        /// </param>
        /// <returns>
        ///     The <see cref="byte" />.
        /// </returns>
        byte DecimalPrecisionFor(Type entity, string propertyName);

        /// <summary>
        ///     Specifies the decimal scale for a property
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property Name.
        /// </param>
        /// <returns>
        ///     The <see cref="byte" />.
        /// </returns>
        byte DecimalScaleFor(Type entity, string propertyName);
    }
}