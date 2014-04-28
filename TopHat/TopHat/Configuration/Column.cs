using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TopHat.Configuration
{
    public class Column
    {
        public string PropertyName { get; set; }

        public string ColumnName { get; set; }

        public Type PropertyType { get; set; }

        public DbType ColumnType { get; set; }

        /// <summary>
        /// Indicates whether the column will be included by default in Query on this type
        /// </summary>
        public bool IncludeByDefault { get; set; }

        public string ColumnTypeString { get; set; }
    }
}