using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashing.Engine.DDL;
using Dashing.Configuration;

namespace Dashing.Tools.Migration
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

        public string GenerateSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors)
        {
            throw new NotImplementedException();
        }

        public string GenerateNaiveSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors)
        {
            warnings = new List<string>();
            errors = new List<string>();

            // naive is simple, drop all tables, recreate
            // TODO Add Warnings
            var sql = new StringBuilder();
            foreach (var map in from)
            {
                sql.Append(this.dropTableWriter.DropTableIfExists(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            foreach (var map in to)
            {
                sql.Append(this.createTableWriter.CreateTable(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            return sql.ToString();
        }

        private void AppendSemiColonIfNecesssary(StringBuilder sql)
        {
            if (sql[sql.Length - 1] != ';')
            {
                sql.Append(";");
            }
        }
    }
}
