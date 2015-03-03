namespace Dashing.Tools.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;
    using Dashing.Engine.DDL;

    public class NaiveMigrator : MigratorBase, IMigrator {
        private readonly ICreateTableWriter createTableWriter;

        private IAlterTableWriter alterTableWriter;

        private readonly IDropTableWriter dropTableWriter;

        public NaiveMigrator(ICreateTableWriter createTableWriter, IDropTableWriter dropTableWriter, IAlterTableWriter alterTableWriter) {
            this.createTableWriter = createTableWriter;
            this.alterTableWriter = alterTableWriter;
            this.dropTableWriter = dropTableWriter;
        }

        /// <summary>
        /// naive is simple, drop all tables, recreate
        /// </summary>
        /// <param name="fromMaps"></param>
        /// <param name="toMaps"></param>
        /// <param name="warnings"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public string GenerateSqlDiff(IEnumerable<IMap> fromMaps, IEnumerable<IMap> toMaps, IAnswerProvider answerProvider, Action<string, object[]> trace, IEnumerable<string> indexesToIgnore, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            throw new InvalidOperationException("The naive migrator has been deprecated");
            var sql = new StringBuilder();
            var from = fromMaps.OrderTopologically().OrderedMaps.ToList();
            var to = toMaps as List<IMap> ?? toMaps.ToList();
            var warningList = new List<string>();
            warnings = warningList;
            errors = new List<string>();

            // segment the changes
            IMap[] removals;
            MigrationPair[] matches;
            var additions = GetTableChanges(to, @from, out removals, out matches);

            // print out adds
            warningList.AddRange(additions.Select(a => string.Format("Adding {0}", a.Table)));

            // print out deletes
            warningList.AddRange(removals.Select(a => string.Format("Removing {0}", a.Table)));

            // look for a shortcut
            foreach (var pair in matches) {
                string message;
                if (pair.RequiresUpdate(out message)) {
                    warningList.Add(string.Format("{0} requires update: {1}", pair.From.Table, message));
                }
                else {
                    from.Remove(pair.From);
                    to.Remove(pair.To);
                    warningList.Add(string.Format("No changes detected for {0}", pair.From.Table));
                }
            }


            // drop tables (ordered topologically, might work if we haven't broken things above)
            foreach (var map in from) {
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
    }
}