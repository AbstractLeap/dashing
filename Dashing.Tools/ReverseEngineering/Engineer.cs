namespace Dashing.Tools.ReverseEngineering {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;
    using Dashing.Tools.SchemaReading;

    public class Engineer : IEngineer {
        private readonly IConvention convention;

        private ModuleBuilder moduleBuilder;

        private readonly IDictionary<string, Type> typeMap;

        private IReverseEngineeringConfiguration configuration;

        /// <summary>
        ///     maps table name to list of many to one columns
        /// </summary>
        private readonly IDictionary<string, IList<IColumn>> manyToOneColumns;

        public Engineer(IEnumerable<KeyValuePair<string, string>> extraPluralizationWords)
            : this(new DefaultConvention(extraPluralizationWords)) {
        }

        public Engineer(IConvention convention) {
            this.convention = convention;
            this.typeMap = new Dictionary<string, Type>();
            this.manyToOneColumns = new Dictionary<string, IList<IColumn>>();
            this.InitTypeGenerator();
        }

        private void InitTypeGenerator() {
            var assemblyName = new AssemblyName("Dashing.ReverseEngineering");
#if COREFX
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#else
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        private Type GenerateType(string name) {
            var className = this.convention.ClassNameFor(name);
            var typeBuilder = this.moduleBuilder.DefineType(className, TypeAttributes.Public);
#if COREFX
            var type = typeBuilder.CreateTypeInfo().GetType();
#else
            var type = typeBuilder.CreateType();
#endif
            this.typeMap.Add(name, type);
            return type;
        }

        public IEnumerable<IMap> ReverseEngineer(
            Database schema,
            ISqlDialect sqlDialect,
            IEnumerable<string> tablesToIgnore,
            IAnswerProvider answerProvider,
            bool fixOneToOnes) {
            if (tablesToIgnore == null) {
                tablesToIgnore = new string[0];
            }

            var maps = new List<IMap>();
            this.configuration = new Configuration(sqlDialect);
            foreach (var table in schema.Tables.Where(t => !tablesToIgnore.Contains(t.Name))) {
                maps.Add(this.MapTable(table, schema, sqlDialect));
            }

            // go back through and add indexes and foreign keys
            foreach (var map in maps) {
                GetIndexesAndForeignKeys(map, schema);
            }

            // go back through and try to spot one-to-one columns
            if (fixOneToOnes) {
                foreach (var map in maps) {
                    FindOneToOnes(map, answerProvider);
                }
            }

            return maps;
        }

        private void FindOneToOnes(IMap map, IAnswerProvider answerProvider) {
            foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne)) {
                var otherMapCandidates =
                    column.Value.ParentMap.Columns.Where(
                        c => c.Value.Type == column.Value.Map.Type && (column.Value.ParentMap != map || c.Key != column.Key)).ToArray();
                if (otherMapCandidates.Length == 0) {
                    continue;
                }
                else if (otherMapCandidates.Length == 1) {
                    column.Value.Relationship = RelationshipType.OneToOne; // one relationship coming back, assume one to one
                    column.Value.OppositeColumn = otherMapCandidates.First().Value;
                }
                else {
                    // we've got more than 1 foreign key coming back - let's ask the user
                    var choices = otherMapCandidates.Select(c => new MultipleChoice<IColumn> { DisplayString = c.Key, Choice = c.Value }).ToList();
                    const string oneToOneText = "No matching column but one-to-one";
                    const string manyToOneText = "No matching column but many-to-one";
                    choices.Add(new MultipleChoice<IColumn> { DisplayString = oneToOneText, Choice = new Column<string> { Name = "One to One" } });
                    choices.Add(new MultipleChoice<IColumn> { DisplayString = manyToOneText, Choice = new Column<string> { Name = "Many to One" } });
                    var oppositeColumn =
                        answerProvider.GetMultipleChoiceAnswer(
                            "The column " + column.Key + " on " + column.Value.Map.Table
                            + " has multiple incoming relationships. Which column on the related table is the other side of the one-to-one relationship?",
                            choices);
                    if (oppositeColumn.DisplayString == manyToOneText) {
                        continue; // many to one
                    }

                    column.Value.Relationship = RelationshipType.OneToOne;
                    if (oppositeColumn.DisplayString != oneToOneText) {
                        column.Value.OppositeColumn = oppositeColumn.Choice;
                    }
                }
            }
        }

        private IColumn BuildOneToManyColumn(IColumn manyToOneColumn, string propName) {
            var col = Activator.CreateInstance(typeof(Column<>).MakeGenericType(typeof(IList<>).MakeGenericType(manyToOneColumn.Type)));
            var manyToOneReverseEngineeredColumn = manyToOneColumn as IReverseEngineeringColumn;
            var oneToManyColumn = col as IColumn;
            oneToManyColumn.Name = propName;
            oneToManyColumn.Relationship = RelationshipType.OneToMany;
            oneToManyColumn.Map =
                manyToOneColumn.Map.Configuration.GetMap(
                    manyToOneReverseEngineeredColumn.TypeMap[manyToOneReverseEngineeredColumn.ForeignKeyTableName]);
            oneToManyColumn.ChildColumn = manyToOneColumn;
            return oneToManyColumn;
        }

        private IMap MapTable(TableDto table, Database schema, ISqlDialect sqlDialect) {
            var type = this.GenerateType(table.Name);
            var map = Activator.CreateInstance(typeof(Map<>).MakeGenericType(type));
            var iMap = map as IMap;
            iMap.Table = table.Name;
            iMap.Configuration = this.configuration;
            foreach (var column in schema.GetColumnsForTable(table.Name)) {
                iMap.Columns.Add(this.MapColumn(iMap, column, schema, sqlDialect));
            }

            this.configuration.AddMap(type, iMap);
            return iMap;
        }

        private void GetIndexesAndForeignKeys(IMap map, Database schema) {
            // try to find foreign keys
            var foreignKeys = new List<ForeignKey>();
            foreach (var foreignKey in schema.GetForeignKeysForTable(map.Table)) {
                var childColumn = map.Columns.Select(c => c.Value).First(c => c.DbName == foreignKey.ColumnName);
                foreignKeys.Add(new ForeignKey(childColumn.ParentMap, childColumn, foreignKey.Name));
            }

            map.ForeignKeys = foreignKeys;

            // try to find indexes
            var indexes = new List<Index>();
            foreach (var index in schema.GetIndexesForTable(map.Table).GroupBy(i => i.Name)) {
                indexes.Add(
                    new Index(
                        map,
                        index.OrderBy(i => i.ColumnId).Select(
                            c =>
                            map.Columns[
                                foreignKeys.Any(f => f.ChildColumn.DbName == c.ColumnName)
                                    ? this.convention.PropertyNameForManyToOneColumnName(c.ColumnName)
                                    : c.ColumnName]).ToList(),
                        index.Key,
                        index.First().IsUnique));
            }

            map.Indexes = indexes;
        }

        private KeyValuePair<string, IColumn> MapColumn(IMap map, ColumnDto column, Database schema, ISqlDialect sqlDialect) {
            // figure out the type
            Type type = sqlDialect.GetTypeFromString(column.DbTypeName, column.Length, column.Precision).GetCLRType();
            var col = (IReverseEngineeringColumn)Activator.CreateInstance(typeof(Column<>).MakeGenericType(type));
            col.ColumnSpecification = column;
            var mapColumn = (IColumn)col;
            mapColumn.DbName = column.Name;
            sqlDialect.UpdateColumnFromSpecification(mapColumn, new ColumnSpecification {
                                                                                            DbTypeName = column.DbTypeName,
                                                                                            Length = column.Length,
                                                                                            Precision = (byte?)column.Precision,
                                                                                            Scale = (byte?)column.Scale
                                                                                        });
            mapColumn.IsAutoGenerated = column.IsAutoGenerated;
            mapColumn.IsExcludedByDefault = false;
            mapColumn.IsIgnored = false;
            mapColumn.IsNullable = column.IsNullable;
            mapColumn.IsPrimaryKey = column.IsPrimaryKey; // HACK - MySql issue with primary keys?
            if (mapColumn.IsPrimaryKey) {
                map.PrimaryKey = mapColumn;
            }

            mapColumn.Map = map;

            // figure out the relationship
            var foreignKey = schema.GetForeignKeyForColumn(column.Name, map.Table);
            if (foreignKey != null) {
                mapColumn.Relationship = RelationshipType.ManyToOne;
                mapColumn.Name = this.convention.PropertyNameForManyToOneColumnName(column.Name);
                col.ForeignKeyTableName = foreignKey.ReferencedTableName;
                col.TypeMap = this.typeMap;
                if (!this.manyToOneColumns.ContainsKey(foreignKey.ReferencedTableName)) {
                    this.manyToOneColumns.Add(foreignKey.ReferencedTableName, new List<IColumn>());
                }

                this.manyToOneColumns[foreignKey.ReferencedTableName].Add(mapColumn);
            }
            else {
                mapColumn.Relationship = RelationshipType.None;
                mapColumn.Name = column.Name;
            }

            return new KeyValuePair<string, IColumn>(mapColumn.Name, mapColumn);
        }
    }
}