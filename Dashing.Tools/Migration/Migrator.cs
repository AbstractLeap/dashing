namespace Dashing.Tools.Migration {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;

    public class Migrator : IMigrator {
        private readonly ICreateTableWriter createTableWriter;

        private IAlterTableWriter alterTableWriter;

        private readonly IDropTableWriter dropTableWriter;

        public Migrator(ICreateTableWriter createTableWriter, IDropTableWriter dropTableWriter, IAlterTableWriter alterTableWriter) {
            this.createTableWriter = createTableWriter;
            this.alterTableWriter = alterTableWriter;
            this.dropTableWriter = dropTableWriter;
        }

        public string GenerateSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            throw new NotImplementedException();
        }

        public string GenerateNaiveSqlDiff(IEnumerable<IMap> from, IEnumerable<IMap> to, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            warnings = new List<string>();
            errors = new List<string>();

            // naive is simple, drop all tables, recreate
            // TODO Add Warnings
            var sql = new StringBuilder();
            foreach (var map in from) {
                sql.Append(this.dropTableWriter.DropTableIfExists(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            foreach (var map in to) {
                sql.Append(this.createTableWriter.CreateTable(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            return sql.ToString();
        }

        private void AppendSemiColonIfNecesssary(StringBuilder sql) {
            if (sql[sql.Length - 1] != ';') {
                sql.Append(";");
            }
        }
    }
}