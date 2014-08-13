namespace Dashing.Tools.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;
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

        /// <summary>
        /// naive is simple, drop all tables, recreate
        /// </summary>
        /// <param name="fromMaps"></param>
        /// <param name="toMaps"></param>
        /// <param name="warnings"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public string GenerateNaiveSqlDiff(IEnumerable<IMap> fromMaps, IEnumerable<IMap> toMaps, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            var sql = new StringBuilder();
            var from = fromMaps as List<IMap> ?? fromMaps.ToList();
            var to = toMaps as List<IMap> ?? toMaps.ToList();
            warnings = new List<string>();
            errors = new List<string>();

            // look for a shortcut
            var pairs = @from.Join(to, f => f.Table, t => t.Table, MigrationPair.Of).ToArray();
            if (!pairs.Any(p => p.RequiresUpdate())) {
                from = from.Except(pairs.Select(p => p.From)).ToList();
                to = to.Except(pairs.Select(p => p.To)).ToList();
            }
                    

            // drop tables
            foreach (var map in from.OrderTopologically()) {
                sql.Append(this.dropTableWriter.DropTableIfExists(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }
            
            // create tables
            foreach (var map in to) {
                sql.Append(this.createTableWriter.CreateTable(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            // add in foreign keys
            foreach (var map in to) {
                var statements = this.createTableWriter.CreateForeignKeys(map);
                foreach (var statement in statements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }

            // add in indexes
            foreach (var map in to) {
                var statements = this.createTableWriter.CreateIndexes(map);
                foreach (var statement in statements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
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