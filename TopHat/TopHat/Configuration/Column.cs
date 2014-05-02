using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TopHat.Configuration
{
    public class Column
    {
        public Column()
        {
            this.IncludeByDefault = true;
        }

        public string PropertyName { get; set; }

        public string ColumnName { get; set; }

        public Type PropertyType { get; set; }

        public DbType ColumnType { get; set; }

        public uint Precision { get; set; }

        public uint Scale { get; set; }

        public uint Length { get; set; }

        /// <summary>
        /// Indicates whether the column will be included by default in Query on this type
        /// </summary>
        public bool IncludeByDefault { get; set; }

        public string ColumnTypeString { get; set; }

        public RelationshipType Relationship { get; set; }
    }
}