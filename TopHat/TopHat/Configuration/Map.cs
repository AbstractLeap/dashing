using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration
{
    public class Map<T> : IMap<T>
    {
        public Map()
        {
            this.Columns = new Dictionary<string, Column>();
            this.Indexes = new List<IList<string>>();
        }

        public Type Type { get; set; }

        public string Table { get; set; }

        public string Schema { get; set; }

        public string PrimaryKey { get; set; }

        public bool IsPrimaryKeyDatabaseGenerated { get; set; }

        public IDictionary<string, Column> Columns { get; set; }

        public IList<IList<string>> Indexes { get; set; }

        public string SqlSelectByPrimaryKey { get; set; }

        public string SqlSelectByPrimaryKeyIncludeAllColumns { get; set; }

        public string SqlInsert { get; set; }

        public string SqlDeleteByPrimaryKey { get; set; }
    }
}