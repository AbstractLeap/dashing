namespace Dashing.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;
    using Dashing.Logging;
    using Dashing.Versioning;

#if COREFX
#endif

    public class Migrator : IMigrator {
        private readonly ISqlDialect dialect;

        private readonly IStatisticsProvider statisticsProvider;

        private readonly ICreateTableWriter createTableWriter;

        private readonly IDropTableWriter dropTableWriter;

        private readonly IAlterTableWriter alterTableWriter;

        private const string NoRename = "__NOTRENAMED";

        private static string[] versionedEntityColumnNames = typeof(IVersionedEntity<>).GetProperties().Select(p => p.Name).ToArray();

        private static readonly ILog Logger = LogProvider.For<Migrator>();

        public Migrator(
            ISqlDialect dialect,
            ICreateTableWriter createTableWriter,
            IAlterTableWriter alterTableWriter,
            IDropTableWriter dropTableWriter,
            IStatisticsProvider statisticsProvider) {
            this.dialect = dialect;
            this.statisticsProvider = statisticsProvider;
            this.createTableWriter = createTableWriter;
            this.dropTableWriter = dropTableWriter;
            this.alterTableWriter = alterTableWriter;
        }

        public string GenerateSqlDiff(
            IEnumerable<IMap> fromMaps,
            IEnumerable<IMap> toMaps,
            IAnswerProvider answerProvider,
            IEnumerable<string> indexesToIgnore,
            IEnumerable<string> tablesToIgnore,
            out IEnumerable<string> warnings,
            out IEnumerable<string> errors) {
            // fetch data for current database
            IDictionary<string, Statistics> currentData = new Dictionary<string, Statistics>();
            if (fromMaps.Any()) {
                currentData = this.statisticsProvider.GetStatistics(fromMaps);
            }

            var sql = new StringBuilder();
            var warningList = new List<string>();
            var errorList = new List<string>();
            var renamePrimaryKeyModifications = new Dictionary<Tuple<string, string>, bool>();
            var from = fromMaps.ToArray();
            var to = toMaps.ToArray();

            // get additions and removals
            var mapComparer = new TableNameEqualityComparer();
            var additions = to.Except(from, mapComparer).Where(m => !tablesToIgnore.Contains(m.Table)).ToList();
            var removals = from.Except(to, mapComparer).ToList();
            var matches = from.Join(to, f => f.Table.ToLowerInvariant(), t => t.Table.ToLowerInvariant(), MigrationPair.Of).ToList();

            // trace output
            Logger.Info("Additions", additions.Select(a => new { a.Table, a.Type.Name }));
            Logger.Info("Removals", removals.Select(a => new { a.Table, a.Type.Name }));
            Logger.Info("Matches", matches.Select(m => new { FromTable = m.From.Table, FromMap = m.From.Type.Name, ToTable = m.To.Table, ToMap = m.To.Type.Name }));

            // look for possible entity name changes
            if (additions.Any() && removals.Any()) {
                // TODO do something a bit more sensible with regards to likelihood of rename
                foreach (var removed in removals.Select(r => r).ToArray()) {
                    // copy the array as we'll update
                    var answer =
                        answerProvider.GetMultipleChoiceAnswer(
                            string.Format((string)"The entity {0} has been removed. If it has been renamed please specify what to:", (object)removed.Type.Name),
                            new[] { new MultipleChoice<string> { DisplayString = "Not renamed - please delete", Choice = NoRename } }.Union(
                                additions.Select(a => new MultipleChoice<string> { Choice = a.Type.Name, DisplayString = a.Type.Name })));
                    if (answer.Choice != NoRename) {
                        // rename the table
                        var renameFrom = removed;
                        var renameTo = additions.First(a => a.Type.Name == answer.Choice);
                        sql.AppendSql(this.alterTableWriter.RenameTable(renameFrom, renameTo));

                        // add to the matches
                        matches.Add(MigrationPair.Of(renameFrom, renameTo));

                        // modify additions and removals
                        removals.Remove(renameFrom);
                        additions.Remove(renameTo);

                        // sort out the primary key
                        var fromPrimaryKey = renameFrom.PrimaryKey;
                        var toPrimaryKey = renameTo.PrimaryKey;
                        if (!AreColumnDefinitionsEqual(fromPrimaryKey, toPrimaryKey)) {
                            if (fromPrimaryKey.DbName != toPrimaryKey.DbName && fromPrimaryKey.DbType == toPrimaryKey.DbType
                                && (!fromPrimaryKey.DbType.TypeTakesLength() || (fromPrimaryKey.MaxLength && toPrimaryKey.MaxLength)
                                    || (fromPrimaryKey.Length == toPrimaryKey.Length))
                                && (!fromPrimaryKey.DbType.TypeTakesPrecisionAndScale()
                                    || (fromPrimaryKey.Precision == toPrimaryKey.Precision && fromPrimaryKey.Scale == toPrimaryKey.Scale))) {
                                // just a change in name
                                sql.AppendSql(this.alterTableWriter.ChangeColumnName(fromPrimaryKey, toPrimaryKey));
                            }
                            else {
                                // ask the question
                                // TODO may things more sensible based on the type
                                var attemptChange =
                                    answerProvider.GetBooleanAnswer(
                                        "The primary key change required for this table rename involves a change "
                                        + (fromPrimaryKey.DbType != toPrimaryKey.DbType ? "of data type" : "of specification")
                                        + ". Would you like to attempt the change? (Selecting No will drop and re-create the column)");
                                if (attemptChange) {
                                    if (fromPrimaryKey.DbName != toPrimaryKey.DbName) {
                                        sql.AppendSql(this.alterTableWriter.ChangeColumnName(fromPrimaryKey, toPrimaryKey));
                                    }

                                    sql.AppendSql(this.alterTableWriter.ModifyColumn(fromPrimaryKey, toPrimaryKey));
                                    renamePrimaryKeyModifications.Add(Tuple.Create(fromPrimaryKey.Map.Type.Name, toPrimaryKey.Map.Type.Name), true);
                                }
                                else {
                                    // drop and re-create
                                    sql.AppendSql(this.alterTableWriter.DropColumn(fromPrimaryKey));
                                    sql.AppendSql(this.alterTableWriter.AddColumn(toPrimaryKey));
                                    renamePrimaryKeyModifications.Add(Tuple.Create(fromPrimaryKey.Map.Type.Name, toPrimaryKey.Map.Type.Name), false);
                                }
                            }
                        }
                    }
                }
            }

            // do removal of foreign keys and indexes that we don't need
            // only do this on remaining tables as drops should be deleted automatically
            foreach (var matchPair in matches) {
                var fkRemovals = matchPair.From.ForeignKeys.Except(matchPair.To.ForeignKeys);
                foreach (var foreignKey in fkRemovals) {
                    sql.AppendSql(this.alterTableWriter.DropForeignKey(foreignKey));
                }

                var indexRemovals = matchPair.From.Indexes.Except(matchPair.To.Indexes);
                foreach (var index in indexRemovals.Where(i => !indexesToIgnore.Contains(this.dialect.GetIndexName(i)))) {
                    sql.AppendSql(this.alterTableWriter.DropIndex(index));
                }
            }

            // do renames of columns
            var columnKeyValuePairEqualityComparer = new ColumnKeyValuePairEqualityComparer();
            var addedProperties = new List<IColumn>();
            foreach (var pair in matches) {
                var fromCols = pair.From.OwnedColumns(true).ToDictionary(c => c.DbName, c => c);
                var toCols = pair.To.OwnedColumns(true).ToDictionary(c => c.DbName, c => c);

                var removedColumns = fromCols.Except(toCols, columnKeyValuePairEqualityComparer);
                var addedColumns = toCols.Except(fromCols, columnKeyValuePairEqualityComparer).ToList();
                if (removedColumns.Any()) {
                    // handle drops and renames
                    foreach (var removal in removedColumns) {
                        if (pair.From.Type.Name != pair.To.Type.Name && removal.Value.IsPrimaryKey) {
                            // ignore this one and get the new pk and remove it as handled above
                            var newPrimaryKey = toCols.Single(c => c.Value.IsPrimaryKey);
                            addedColumns.Remove(newPrimaryKey);
                            continue;
                        }

                        if (addedColumns.Any()) {
                            var answer =
                                answerProvider.GetMultipleChoiceAnswer(
                                    string.Format(
                                        (string)"The property {0} has been removed. If it has been renamed please specify what to:",
                                        (object)removal.Value.Name),
                                    new[] { new MultipleChoice<string> { DisplayString = "Not renamed - please delete", Choice = NoRename } }.Union(
                                        addedColumns.Select(a => new MultipleChoice<string> { Choice = a.Value.Name, DisplayString = a.Value.Name })));
                            if (answer.Choice == NoRename) {
                                // drop the column
                                if (pair.From.Type.Name != pair.To.Type.Name) {
                                    removal.Value.Map = pair.To; // want to delete from the correctly named table in the event of a rename
                                }

                                sql.AppendSql(this.alterTableWriter.DropColumn(removal.Value));
                            }
                            else {
                                // rename the column
                                var toColumn = addedColumns.First(c => c.Value.Name == answer.Choice);
                                sql.AppendSql(this.alterTableWriter.ChangeColumnName(removal.Value, toColumn.Value));

                                // if need be perform a modify statement
                                if (this.RequiresColumnSpecificationChange(removal.Value, toColumn.Value)) {
                                    sql.AppendSql(this.alterTableWriter.ModifyColumn(removal.Value, toColumn.Value));
                                }

                                // remove the column from the additions
                                addedColumns.Remove(toColumn);
                            }
                        }
                        else {
                            // drop the column
                            if (pair.From.Type.Name != pair.To.Type.Name) {
                                removal.Value.Map = pair.To; // want to delete from the correctly named table in the event of a rename
                            }

                            sql.AppendSql(this.alterTableWriter.DropColumn(removal.Value));
                        }
                    }
                }

                // go through existing columns and handle modifications
                foreach (var fromProp in pair.From.Columns) {
                    Logger.Debug("Looking for modifications to column", fromProp.Value.Name, pair.From.Table);
                    var matchingToProp = pair.To.Columns.Select(p => p.Value).FirstOrDefault(p => p.Name == fromProp.Key);
                    if (matchingToProp != null) {
                        if (this.RequiresColumnSpecificationChange(fromProp.Value, matchingToProp)) {
                            // check for potential errors
                            if (fromProp.Value.DbType != matchingToProp.DbType) {
                                bool skipQuestion = false;
                                bool wasPrimaryKeyDroppedAndRecreated = false;
                                if ((fromProp.Value.Relationship == RelationshipType.ManyToOne || fromProp.Value.Relationship == RelationshipType.OneToOne)
                                    && (matchingToProp.Relationship == RelationshipType.ManyToOne || matchingToProp.Relationship == RelationshipType.OneToOne)) {
                                    var renamePrimaryKeyModificationsKey =
                                    Tuple.Create(
                                        fromProp.Value.Relationship == RelationshipType.ManyToOne
                                            ? fromProp.Value.ParentMap.Type.Name
                                            : fromProp.Value.OppositeColumn.ParentMap.Type.Name,
                                        matchingToProp.Relationship == RelationshipType.ManyToOne
                                            ? matchingToProp.ParentMap.Type.Name
                                            : matchingToProp.OppositeColumn.Map.Type.Name);
                                
                                    if (renamePrimaryKeyModifications.ContainsKey(renamePrimaryKeyModificationsKey)) {
                                        // skip the question as we've already attempted the modify for the pk so may as well here as well!
                                        skipQuestion = true;
                                        wasPrimaryKeyDroppedAndRecreated = !renamePrimaryKeyModifications[renamePrimaryKeyModificationsKey];
                                    }
                                }

                                bool dropAndRecreate = wasPrimaryKeyDroppedAndRecreated;
                                if (!skipQuestion) {
                                    dropAndRecreate =
                                        !answerProvider.GetBooleanAnswer(
                                            string.Format(
                                                "Attempting to change DbType for property {0} on {1} from {2} to {3}. Would you like to attempt the change? (selecting \"No\" will drop and re-create the column)",
                                                matchingToProp.Name,
                                                matchingToProp.Map.Type.Name,
                                                fromProp.Value.DbType,
                                                matchingToProp.DbType));
                                }

                                if (dropAndRecreate) {
                                    sql.AppendSql(this.alterTableWriter.DropColumn(matchingToProp));
                                    sql.AppendSql(this.alterTableWriter.AddColumn(matchingToProp));
                                    continue;
                                }

                                warningList.Add(
                                    string.Format(
                                        "Changing DB Type is not guaranteed to work: {0} on {1}",
                                        fromProp.Value.Name,
                                        fromProp.Value.Map.Type.Name));
                            }

                            if ((this.RequiresLengthChange(fromProp.Value, matchingToProp)
                                 && (fromProp.Value.MaxLength || fromProp.Value.Length < matchingToProp.Length))
                                || (this.RequiresPrecisionOrScaleChange(fromProp.Value, matchingToProp)
                                    && (fromProp.Value.Precision > matchingToProp.Precision || fromProp.Value.Scale > matchingToProp.Scale))) {
                                warningList.Add(
                                    string.Format(
                                        "{0} on {1} is having its precision, scale or length reduced. This may result in loss of data",
                                        fromProp.Value.Name,
                                        fromProp.Value.Map.Type.Name));
                            }

                            sql.AppendSql(this.alterTableWriter.ModifyColumn(fromProp.Value, matchingToProp));
                        }
                        else {
                            if ((matchingToProp.Relationship == RelationshipType.ManyToOne || matchingToProp.Relationship == RelationshipType.OneToOne)
                                && (fromProp.Value.Relationship == RelationshipType.ManyToOne
                                    || fromProp.Value.Relationship == RelationshipType.OneToOne)
                                && !fromProp.Value.Type.Name.Equals(matchingToProp.Type.Name, StringComparison.OrdinalIgnoreCase) && currentData != null
                                && currentData[matchingToProp.Map.Type.Name.ToLowerInvariant()].HasRows) {
                                warningList.Add(
                                    string.Format(
                                        "Property {0} on {1} has changed type but the column was not dropped. There is data in that table, please empty that column if necessary",
                                        matchingToProp.Name,
                                        matchingToProp.Map.Type.Name));
                            }
                        }
                    }
                }

                // add the added columns to the addedProperties list
                addedProperties.AddRange(addedColumns.Select(c => c.Value));
            }

            // do deletes of entities
            foreach (var removal in removals) {
                sql.AppendSql(this.dropTableWriter.DropTable(removal));
            }

            // do additions of entities
            foreach (var addition in additions) {
                sql.AppendSql(this.createTableWriter.CreateTable(addition));
            }

            // do additions of properties
            if (addedProperties.Any()) {
                foreach (var newPropGrouping in addedProperties.GroupBy(p => p.Map.Type)) {
                    var thisMapAddedProperies = new List<IColumn>();
                    foreach (var newProp in newPropGrouping) {
                        // check for relationships where the related table is not empty and the prop is not null
                        if ((newProp.Relationship == RelationshipType.ManyToOne || newProp.Relationship == RelationshipType.OneToOne) && !newProp.IsNullable && string.IsNullOrWhiteSpace(newProp.GetDefault(this.dialect)) && currentData.ContainsKey(newProp.Map.Type.Name.ToLowerInvariant()) && currentData[newProp.Map.Type.Name.ToLowerInvariant()]
                                .HasRows) {
                            var foreignKeyPrimaryKeyType = newProp.Relationship == RelationshipType.ManyToOne
                                                               ? newProp.ParentMap.PrimaryKey.Type
                                                               : newProp.OppositeColumn.Map.PrimaryKey.Type;
                            var answer = answerProvider.GetType()
                                                       .GetMethod("GetAnswer")
                                                       .MakeGenericMethod(foreignKeyPrimaryKeyType)
                                                       .Invoke(answerProvider, new object[] { string.Format("You are adding a property {0} on {1} that has a foreign key to {2} (which is non-empty) with primary key type {3}. Please specify a default value for the column:", newProp.Name, newProp.Map.Type.Name, newProp.Type.Name, foreignKeyPrimaryKeyType) });
                            newProp.Default = answer.ToString();
                        }

                        thisMapAddedProperies.Add(newProp);
                    }

                    sql.AppendSql(this.alterTableWriter.AddColumn(thisMapAddedProperies.ToArray()));
                }
            }

            // add in ssytem versioning where necessary
            foreach (var migrationPair in matches) {
                if (migrationPair.To.Type.IsVersionedEntity() && string.IsNullOrWhiteSpace(migrationPair.From.HistoryTable)) {
                    sql.AppendSql(this.alterTableWriter.AddSystemVersioning(migrationPair.To));
                }
            }

            // add in new foreign keys for additions
            foreach (var map in additions) {
                var statements = this.createTableWriter.CreateForeignKeys(map);
                foreach (var statement in statements) {
                    sql.AppendSql(statement);
                }
            }

            // add in new indexes for additions
            foreach (var map in additions) {
                var statements = this.createTableWriter.CreateIndexes(map);
                foreach (var statement in statements) {
                    sql.AppendSql(statement);
                }
            }

            // add in missing foreign keys and indexes
            foreach (var matchPair in matches) {
                var fkAdditions = matchPair.To.ForeignKeys.Except(matchPair.From.ForeignKeys);
                var fkStatements = this.createTableWriter.CreateForeignKeys(fkAdditions);
                foreach (var statement in fkStatements) {
                    sql.AppendSql(statement);
                }

                var indexAdditions = matchPair.To.Indexes.Except(matchPair.From.Indexes);
                var indexStatements = this.createTableWriter.CreateIndexes(indexAdditions);
                foreach (var statement in indexStatements) {
                    sql.AppendSql(statement);
                }
            }

            warnings = warningList;
            errors = errorList;
            return sql.ToString();
        }

        private bool AreColumnDefinitionsEqual(IColumn left, IColumn right) {
            return !this.RequiresColumnNameChange(left, right) && !this.RequiresColumnSpecificationChange(left, right);
        }

        private bool RequiresColumnNameChange(IColumn from, IColumn to) {
            return from.DbName != to.DbName;
        }

        private bool RequiresColumnSpecificationChange(IColumn from, IColumn to) {
            // handle versioned tables
            if (to.Map.Type.IsVersionedEntity() && versionedEntityColumnNames.Contains(to.Name)) {
                return false;
            }

            return !this.dialect.GetColumnSpecification(to).Equals(this.dialect.GetColumnSpecification(from))
                   || from.IsNullable != to.IsNullable || (from.GetDefault(this.dialect) ?? string.Empty) != (to.GetDefault(this.dialect) ?? string.Empty) || from.IsAutoGenerated != to.IsAutoGenerated;
        }

        private bool RequiresLengthChange(IColumn from, IColumn to) {
            return from.DbType.TypeTakesLength() && (from.MaxLength != to.MaxLength || (!to.MaxLength && from.Length != to.Length));
        }

        private bool RequiresPrecisionOrScaleChange(IColumn from, IColumn to) {
            return from.DbType.TypeTakesPrecisionAndScale() && (from.Precision != to.Precision || from.Scale != to.Scale);
        }
    }
}