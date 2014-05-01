using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.SqlWriter
{
    internal struct SqlWriterResult
    {
        public string Sql { get; set; }

        public dynamic Parameters { get; set; }
    }
}