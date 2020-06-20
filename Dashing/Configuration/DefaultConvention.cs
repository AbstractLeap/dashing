﻿namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Dashing.Extensions;

    /// <summary>
    ///     The default convention.
    /// </summary>
    //// Dear ReSharper, these things were done on purpose so that people can extend off this
    //// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    //// ReSharper disable once MemberCanBePrivate.Global
    public class DefaultConvention : IConvention {
        private readonly ushort stringLength;

        private readonly byte decimalPrecision;

        private readonly byte decimalScale;

        private readonly byte dateTime2Precision;

        private readonly bool isCollectionInstantiationAutomatic;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultConvention" /> class.
        /// </summary>
        /// <param name="stringLength">
        ///     The string length.
        /// </param>
        /// <param name="decimalPrecision">
        ///     The decimal precision.
        /// </param>
        /// <param name="decimalScale">
        ///     The decimal scale.
        /// </param>
        /// <param name="dateTime2Precision">
        ///     The datetime2 precisions
        /// </param>
        public DefaultConvention(ushort stringLength = 255, byte decimalPrecision = 18, byte decimalScale = 10, byte dateTime2Precision = 2, bool isCollectionInstantiationAutomatic = false) {
            this.stringLength = stringLength;
            this.decimalPrecision = decimalPrecision;
            this.decimalScale = decimalScale;
            this.dateTime2Precision = dateTime2Precision;
            this.isCollectionInstantiationAutomatic = isCollectionInstantiationAutomatic;
        }

        /// <inheritdoc />
        public virtual bool IsOwned(Type entityType) {
            // by default we treat entities without primary keys as owned
            return !this.GetPrimaryKeyCandidates(
                           entityType,
                           entityType.GetProperties()
                                     .Select(p => p.Name))
                       .Any();
        }

        /// <summary>
        ///     The table for.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public virtual string TableFor(Type entity) {
            return entity.Name.Pluralize();
        }

        /// <summary>
        /// Identifies the table name for the history table relating to this temporal table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual  string HistoryTableFor(Type entity) {
            return this.TableFor(entity) + "History";
        }

        /// <summary>
        ///     The schema for.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public virtual string SchemaFor(Type entity) {
            return null;
        }

        /// <summary>
        ///     The primary key of.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyNames">
        ///     The array of property names to choose from.
        /// </param>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public virtual string PrimaryKeyFor(Type entity, IEnumerable<string> propertyNames) {
            return this.GetPrimaryKeyCandidates(entity, propertyNames)
                       .OrderBy(c => c.Score)
                       .FirstOrDefault()
                       .PropertyName;
        }

        private IEnumerable<PrimaryKeyCandidate> GetPrimaryKeyCandidates(Type entity, IEnumerable<string> propertyNames) {
            var primaryKeyCandidates = propertyNames.Select(pn => this.ScorePrimaryKeyCandidate(pn, entity.Name + "Id", "Id"))
                                                    .Where(c => c.Score > 0);
            return primaryKeyCandidates;
        }

        private PrimaryKeyCandidate ScorePrimaryKeyCandidate(string propertyName, params string[] orderedMatches) {
            for (int i = 0, c = orderedMatches.Length; i < c; ++i) {
                if (propertyName.Equals(orderedMatches[i], StringComparison.OrdinalIgnoreCase)) {
                    return new PrimaryKeyCandidate(propertyName, 1 + i);
                }
            }

            return new PrimaryKeyCandidate(propertyName, 0);
        }

        private struct PrimaryKeyCandidate {
            public readonly string PropertyName;
                   
            public readonly int Score;
                   
            public PrimaryKeyCandidate(string propertyName, int score)
                : this() {
                this.PropertyName = propertyName;
                this.Score = score;
            }
        }

        /// <summary>
        ///     The is primary key auto generated.
        /// </summary>
        /// <param name="primaryKeyColumn">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public virtual bool IsPrimaryKeyAutoGenerated(IColumn primaryKeyColumn) {
            if (primaryKeyColumn.Type == typeof(int) || primaryKeyColumn.Type == typeof(long) || primaryKeyColumn.Type == typeof(Guid)) {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     The string length for.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <returns>
        ///     The <see cref="ushort" />.
        /// </returns>
        public virtual  ushort StringLengthFor(Type entity, string propertyName) {
            return this.stringLength;
        }

        /// <summary>
        ///     The decimal precision for.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <returns>
        ///     The <see cref="byte" />.
        /// </returns>
        public virtual  byte DecimalPrecisionFor(Type entity, string propertyName) {
            return this.decimalPrecision;
        }

        /// <summary>
        ///     The decimal scale for.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <returns>
        ///     The <see cref="byte" />.
        /// </returns>
        public virtual  byte DecimalScaleFor(Type entity, string propertyName) {
            return this.decimalScale;
        }

        /// <summary>
        /// The datetime2 precision
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual  byte DateTime2PrecisionFor(Type entity, string propertyName) {
            return this.dateTime2Precision;
        }

        /// <summary>
        /// Specifies whether a column should be auto-initialised via the contructor by weaving
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual  bool IsCollectionInstantiationAutomatic(Type entity, string propertyName) {
            return this.isCollectionInstantiationAutomatic;
        }

        /// <summary>
        /// Maps a .Net type to a database type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual DbType GetDbTypeFor(Type type) {
            return type.GetDbType();
        }

        /// <summary>
        /// Specifies whether the column is nullable in the case that it's many to one
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsManyToOneNullable(Type entity, string propertyName) {
            return true;
        }

        /// <inheritdoc />
        public bool IsOwnedPropertyNullable(Type entityType, string propertyName) {
            return true;
        }
    }
}