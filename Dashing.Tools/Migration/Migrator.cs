namespace Dashing.Tools.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Extensions;

    public class Migrator : MigratorBase, IMigrator {
        private readonly ICreateTableWriter createTableWriter;

        private readonly IAlterTableWriter alterTableWriter;

        private readonly IDropTableWriter dropTableWriter;

        public Migrator(
            ICreateTableWriter createTableWriter,
            IDropTableWriter dropTableWriter,
            IAlterTableWriter alterTableWriter) {
            this.createTableWriter = createTableWriter;
            this.alterTableWriter = alterTableWriter;
            this.dropTableWriter = dropTableWriter;
        }

        public string GenerateSqlDiff(
            IEnumerable<IMap> fromMaps,
            IEnumerable<IMap> toMaps,
            out IEnumerable<string> warnings,
            out IEnumerable<string> errors) {
            var sql = new StringBuilder();
            var from = fromMaps.OrderTopologically().ToList();
            var to = toMaps as List<IMap> ?? toMaps.ToList();
            IList<string> warningList = new List<string>();
            IList<string> errorList = new List<string>();

            // segment the changes
            IMap[] removals;
            MigrationPair[] matches;
            var additions = GetTableChanges(to, @from, out removals, out matches);

            // TODO table name changes i.e. look for classes that appear to have changed name

            // do removal of foreign keys and indexes that we don't need
            // only do this on remaining tables as drops should be deleted automatically
            foreach (var matchPair in matches) {
                var fkRemovals = matchPair.From.ForeignKeys.Except(matchPair.To.ForeignKeys);
                foreach (var foreignKey in fkRemovals) {
                    sql.Append(this.alterTableWriter.DropForeignKey(foreignKey));
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }

                var indexRemovals = matchPair.From.Indexes.Except(matchPair.To.Indexes);
                foreach (var index in indexRemovals) {
                    sql.AppendLine(this.alterTableWriter.DropIndex(index));
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }

            // do creates first as other changes may depend on them
            foreach (var map in additions) {
                sql.Append(this.createTableWriter.CreateTable(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            // next do changes
            IList<IColumn> newForeignKeyColumns = new List<IColumn>();
            foreach (var match in matches) {
                string message;
                if (match.RequiresUpdate(out message)) {
                    warningList.Add(
                        string.Format("{0} requires update: {1}", match.From.Table, message));
                    this.GenerateMapDiff(match, sql, newForeignKeyColumns, warningList, errorList);
                }
            }

            // removals
            foreach (var map in removals) {
                sql.AppendLine(this.dropTableWriter.DropTableIfExists(map));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            // add in new foreign keys for additions
            foreach (var map in additions) {
                var statements = this.createTableWriter.CreateForeignKeys(map);
                foreach (var statement in statements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }

            // add in new indexes for additions
            foreach (var map in additions) {
                var statements = this.createTableWriter.CreateIndexes(map);
                foreach (var statement in statements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }

            // add in missing foreign keys and indexes
            foreach (var matchPair in matches) {
                var fkAdditions = matchPair.To.ForeignKeys.Except(matchPair.From.ForeignKeys);
                var fkStatements = this.createTableWriter.CreateForeignKeys(fkAdditions);
                foreach (var statement in fkStatements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }

                var indexAdditions = matchPair.To.Indexes.Except(matchPair.From.Indexes);
                var indexStatements = this.createTableWriter.CreateIndexes(indexAdditions);
                foreach (var statement in indexStatements) {
                    sql.Append(statement);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }

            errors = errorList;
            warnings = warningList;
            return sql.ToString();
        }

        private void GenerateMapDiff(
            MigrationPair match,
            StringBuilder sql,
            IList<IColumn> newForeignKeyColumns,
            IList<string> warnings,
            IList<string> errors) {
            // try to figure out additions, removals, changes
            var toColumns = match.To.OwnedColumns(true).ToDictionary(c => c.DbName, c => c);
            var fromColumns = match.From.OwnedColumns(true).ToDictionary(c => c.DbName, c => c);

            var addedColumnDbNames = toColumns.Keys.Except(fromColumns.Keys).ToList();
            var removedColumnDbNames = fromColumns.Keys.Except(toColumns.Keys).ToList();
            var nameChangedDbNames = new List<Tuple<string, string>>();

            // try to find some manytoone references with a changed type
            foreach (
                var fromColumn in
                    fromColumns.Where(k => k.Value.Relationship == RelationshipType.ManyToOne)) {
                var matchingToColumn =
                    toColumns.Select(c => c.Value).SingleOrDefault(c => c.Name == fromColumn.Value.Name);
                if (matchingToColumn != null && fromColumn.Value.Type.Name != matchingToColumn.Type.Name) {
                    removedColumnDbNames.Add(fromColumn.Key);
                    addedColumnDbNames.Add(matchingToColumn.DbName);
                }
            }

            // see if we can change these in to changed names
            if (removedColumnDbNames.Any()) {
                foreach (var dbName in addedColumnDbNames) {
                    var addedColumn = toColumns[dbName];
                    foreach (var removedColumnDbName in removedColumnDbNames) {
                        var removedColumn = fromColumns[removedColumnDbName];
                        if (this.IsPotentialNameChange(addedColumn, removedColumn)) {
                            addedColumnDbNames.Remove(addedColumn.DbName);
                            removedColumnDbNames.Remove(removedColumn.DbName);
                            nameChangedDbNames.Add(
                                Tuple.Create(removedColumn.DbName, addedColumn.DbName));
                            break;
                        }
                    }
                }
            }

            // ok, now we have a list of added columns, removed columns, potential name changes 
            // and all the others are either the same or updated
            foreach (var addedColumnDbName in addedColumnDbNames) {
                // addition of column
                sql.AppendLine(this.alterTableWriter.AddColumn(toColumns[addedColumnDbName]));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();

                if (toColumns[addedColumnDbName].Relationship == RelationshipType.ManyToOne) {
                    newForeignKeyColumns.Add(toColumns[addedColumnDbName]);
                }
            }

            // now do removals of columns
            foreach (var removedColumnDbName in removedColumnDbNames) {
                sql.AppendLine(this.alterTableWriter.DropColumn(fromColumns[removedColumnDbName]));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            // now do name changes - we do the name change now and any change of type later
            foreach (var changedColumnDbName in nameChangedDbNames) {
                sql.AppendLine(
                    this.alterTableWriter.ChangeColumnName(
                        fromColumns[changedColumnDbName.Item1],
                        toColumns[changedColumnDbName.Item2]));
                this.AppendSemiColonIfNecesssary(sql);
                sql.AppendLine();
            }

            // right, now let's look for changes to column specifications
            var existingColumnDbNames =
                nameChangedDbNames.Select(t => t.Item2)
                                  .Union(fromColumns.Keys.Intersect(toColumns.Keys))
                                  .ToArray();
            foreach (var existingColumnDbName in existingColumnDbNames) {
                var toColumn = toColumns[existingColumnDbName];
                var fromColumn = fromColumns[existingColumnDbName];
                string changeColumnSql;
                if (this.ColumnHasChanged(
                    fromColumn,
                    toColumn,
                    out changeColumnSql,
                    ref warnings,
                    ref errors)) {
                    sql.AppendLine(changeColumnSql);
                    this.AppendSemiColonIfNecesssary(sql);
                    sql.AppendLine();
                }
            }
        }

        private bool ColumnHasChanged(
            IColumn fromColumn,
            IColumn toColumn,
            out string sql,
            ref IList<string> warnings,
            ref IList<string> errors) {
            // first check for manytoonecolumns that have changed type as we drop and recreate those
            if (fromColumn.Relationship == RelationshipType.ManyToOne
                && toColumn.Relationship == RelationshipType.ManyToOne
                && fromColumn.Type != toColumn.Type) {
                sql = string.Empty;
                return false;
            }

            if (fromColumn.DbType != toColumn.DbType || fromColumn.IsNullable != toColumn.IsNullable
                || (fromColumn.Length != toColumn.Length && fromColumn.Map.Configuration.Engine.SqlDialect.TypeTakesLength(toColumn.DbType))
                || ((fromColumn.Precision != toColumn.Precision || fromColumn.Scale != toColumn.Scale) && fromColumn.Map.Configuration.Engine.SqlDialect.TypeTakesPrecisionAndScale(toColumn.DbType))) {
                // check for potential errors
                if (toColumn.Length < fromColumn.Length || toColumn.Precision < fromColumn.Precision
                    || toColumn.Scale < fromColumn.Scale) {
                    warnings.Add(
                        string.Format(
                            "{0} on {1} is having its precision, scale or length reduced. This may result in loss of data",
                            toColumn.Name,
                            toColumn.Map.Type.Name));
                }

                if ((toColumn.IsPrimaryKey && !fromColumn.IsPrimaryKey)
                    || (fromColumn.IsPrimaryKey && !toColumn.IsPrimaryKey)) {
                    errors.Add(
                        "Changing the PK is not currently supported (on type "
                        + toColumn.Map.Type.Name);
                }

                if (!toColumn.IsNullable && fromColumn.IsNullable) {
                    warnings.Add(
                        string.Format(
                            "The column {0} on {1} is being changed to non-nullable. Missing values will use the default value",
                            toColumn.Name,
                            toColumn.Map.Type.Name));
                }

                if (fromColumn.DbType != toColumn.DbType) {
                    // TODO make some better decisions about whether a DBType change could happen
                    warnings.Add(
                        string.Format(
                            "Changing DB Type is not guaranteed to work: {0} on {1}",
                            toColumn.Name,
                            toColumn.Map.Type.Name));
                }

                sql = this.alterTableWriter.ModifyColumn(fromColumn, toColumn);
                return true;
            }

            sql = string.Empty;
            return false;
        }

        private bool IsPotentialNameChange(IColumn addedColumn, IColumn removedColumn) {
            // if they're both primary keys then the name was probably changed?!
            if (addedColumn.IsPrimaryKey) {
                return removedColumn.IsPrimaryKey;
            }

            // if they're a different relationship we shouldn't rename
            if (addedColumn.Relationship != removedColumn.Relationship) {
                return false;
            }

            // if foreign key and same type then probably a rename
            if (addedColumn.Relationship == RelationshipType.ManyToOne) {
                return addedColumn.Type == removedColumn.Type;
            }

            // if the type matches regardless of nullability then go for it i.e. check DbType
            return addedColumn.DbType == removedColumn.DbType;
        }
    }
}