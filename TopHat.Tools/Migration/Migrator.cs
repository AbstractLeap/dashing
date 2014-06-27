using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TopHat.Engine.DDL;

namespace TopHat.Tools.Migration
{
    public class Migrator : IMigrator
    {
        private ICreateTableWriter createTableWriter;
        private IAlterTableWriter alterTableWriter;
        private IDropTableWriter dropTableWriter;

        public Migrator(ICreateTableWriter createTableWriter,
            IDropTableWriter dropTableWriter,
            IAlterTableWriter alterTableWriter)
        {
            this.createTableWriter = createTableWriter;
            this.alterTableWriter = alterTableWriter;
            this.dropTableWriter = dropTableWriter;
        }

        public string GenerateSqlDiff(Configuration.IConfiguration from, Configuration.IConfiguration to, out IEnumerable<string> warnings, out IEnumerable<string> errors)
        {
            throw new NotImplementedException();
        }
    }
}
