namespace Dashing.Tools.ReverseEngineering {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    using DatabaseSchemaReader.DataSchema;

    public class Engineer : IEngineer {
        private readonly IConvention convention;

        private ModuleBuilder moduleBuilder;

        private readonly IDictionary<string, Type> typeMap;

        private IReverseEngineeringConfiguration configuration;

        /// <summary>
        ///     maps table name to list of many to one columns
        /// </summary>
        private readonly IDictionary<string, IList<IColumn>> manyToOneColumns;

        public Engineer(string extraPluralizationWords)
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
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        private Type GenerateType(string name) {
            var className = this.convention.ClassNameFor(name);
            var typeBuilder = this.moduleBuilder.DefineType(className, TypeAttributes.Public);
            var type = typeBuilder.CreateType();
            this.typeMap.Add(name, type);
            return type;
        }

        public IEnumerable<IMap> ReverseEngineer(
            DatabaseSchema schema,
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
                maps.Add(this.MapTable(table));
            }

            // go back through and add indexes and foreign keys
            foreach (var map in maps) {
                GetIndexesAndForeignKeys(schema.Tables.First(t => t.Name == map.Table), map);
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

        private IMap MapTable(DatabaseTable table) {
            var type = this.GenerateType(table.Name);
            var map = Activator.CreateInstance(typeof(Map<>).MakeGenericType(type));
            var iMap = map as IMap;
            iMap.Table = table.Name;
            iMap.Configuration = this.configuration;
            foreach (var column in table.Columns) {
                iMap.Columns.Add(this.MapColumn(iMap, column));
            }

            this.configuration.AddMap(type, iMap);
            return iMap;
        }

        private void GetIndexesAndForeignKeys(DatabaseTable table, IMap map) {
            // try to find foreign keys
            var foreignKeys = new List<ForeignKey>();
            foreach (var foreignKey in table.ForeignKeys) {
                var childColumn = map.Columns.Select(c => c.Value).First(c => c.DbName == foreignKey.Columns.First());
                foreignKeys.Add(new ForeignKey(childColumn.ParentMap, childColumn, foreignKey.Name));
            }

            map.ForeignKeys = foreignKeys;

            // try to find indexes
            var indexes = new List<Index>();
            foreach (var index in table.Indexes) {
                if (!index.IsUniqueKeyIndex(table)) {
                    indexes.Add(
                        new Index(
                            map,
                            index.Columns.Select(
                                c =>
                                map.Columns[
                                    foreignKeys.Any(f => f.ChildColumn.DbName == c.Name)
                                        ? this.convention.PropertyNameForManyToOneColumnName(c.Name)
                                        : c.Name]).ToList(),
                            index.Name,
                            index.IsUnique));
                }
            }

            map.Indexes = indexes;
        }

        private KeyValuePair<string, IColumn> MapColumn(IMap map, DatabaseColumn column) {
            // figure out the type
            Type type;
            if (column.DataType == null) {
                // HACK throw an exception? log out as a warning??
                type = typeof(string);
            }
            else {
                type = Type.GetType(column.DataType.NetDataType);
            }

            var col = Activator.CreateInstance(typeof(Column<>).MakeGenericType(type));
            var mapColumn = col as IColumn;

            mapColumn.DbName = column.Name;
            mapColumn.DbType = type.GetDbType();
            mapColumn.IsAutoGenerated = column.IsAutoNumber;
            mapColumn.IsExcludedByDefault = false;
            mapColumn.IsIgnored = false;
            mapColumn.IsNullable = column.Nullable;
            mapColumn.IsPrimaryKey = column.IsPrimaryKey || column.IsPrimaryKey; // HACK - MySql issue with primary keys?
            if (mapColumn.IsPrimaryKey) {
                map.PrimaryKey = mapColumn;
            }

            if (column.Length.HasValue) {
                if (column.Length == -1) {
                    // max
                    mapColumn.MaxLength = true;
                }
                else {
                    mapColumn.Length = (ushort)column.Length.Value;
                }
            }

            if (column.Precision.HasValue) {
                mapColumn.Precision = (byte)column.Precision.Value;
            }

            if (column.Scale.HasValue) {
                mapColumn.Scale = (byte)column.Scale.Value;
            }

            mapColumn.Map = map;

            // figure out the relationship
            if (column.IsForeignKey) {
                mapColumn.Relationship = RelationshipType.ManyToOne;
                mapColumn.Name = this.convention.PropertyNameForManyToOneColumnName(column.Name);
                var iReverseEngineeringColumn = col as IReverseEngineeringColumn;
                iReverseEngineeringColumn.ForeignKeyTableName = column.ForeignKeyTableName;
                iReverseEngineeringColumn.TypeMap = this.typeMap;
                if (!this.manyToOneColumns.ContainsKey(column.ForeignKeyTableName)) {
                    this.manyToOneColumns.Add(column.ForeignKeyTableName, new List<IColumn>());
                }

                this.manyToOneColumns[column.ForeignKeyTableName].Add(mapColumn);
            }
            else {
                mapColumn.Relationship = RelationshipType.None;
                mapColumn.Name = column.Name;
            }

            return new KeyValuePair<string, IColumn>(mapColumn.Name, mapColumn);
        }
    }
}