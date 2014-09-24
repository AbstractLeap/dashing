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
        /// maps table name to list of many to one columns
        /// </summary>
        private IDictionary<string, IList<IColumn>> manyToOneColumns;

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
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);
            this.moduleBuilder =
                assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        private Type GenerateType(string name) {
            var className = this.convention.ClassNameFor(name);
            var typeBuilder = this.moduleBuilder.DefineType(className, TypeAttributes.Public);
            var type = typeBuilder.CreateType();
            this.typeMap.Add(name, type);
            return type;
        }

        public IEnumerable<IMap> ReverseEngineer(DatabaseSchema schema, ISqlDialect sqlDialect, IEnumerable<string> tablesToIgnore) {
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

            // now we go through and add onetomany columns so that topological ordering works
            foreach (var tableWithManyToOne in this.manyToOneColumns) {
                foreach (var manyToOneColumn in tableWithManyToOne.Value) {
                    var propName = Guid.NewGuid().ToString("N");
                    manyToOneColumn.Map.Columns.Add(
                        propName,
                        this.BuildOneToManyColumn(manyToOneColumn, propName));
                }
            }

            return maps;
        }

        private IColumn BuildOneToManyColumn(IColumn manyToOneColumn, string propName) {
            var col = Activator.CreateInstance(typeof(Column<>).MakeGenericType(typeof(IList<>).MakeGenericType(manyToOneColumn.Type)));
            var manyToOneReverseEngineeredColumn = manyToOneColumn as IReverseEngineeringColumn;
            var oneToManyColumn = col as IColumn;
            oneToManyColumn.Name = propName;
            oneToManyColumn.Relationship = RelationshipType.OneToMany;
            oneToManyColumn.Map =
                manyToOneColumn.Map.Configuration.GetMap(
                    manyToOneReverseEngineeredColumn.TypeMap[
                        manyToOneReverseEngineeredColumn.ForeignKeyTableName]);
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
            // try to find indexes
            var indexes = new List<Index>();
            foreach (var index in table.Indexes) {
                if (!index.IsUniqueKeyIndex(table)) {
                    indexes.Add(
                        new Index(
                            map,
                            index.Columns.Select(c => map.Columns[this.convention.PropertyNameForManyToOneColumnName(c.Name)]).ToList(),
                            index.Name,
                            index.IsUnique));
                }
            }

            map.Indexes = indexes;

            // try to find foreign keys
            var foreignKeys = new List<ForeignKey>();
            foreach (var foreignKey in table.ForeignKeys) {
                var childColumn =
                    map.Columns.Select(c => c.Value).First(c => c.DbName == foreignKey.Columns.First());
                foreignKeys.Add(new ForeignKey(childColumn.ParentMap, childColumn, foreignKey.Name));
            }

            map.ForeignKeys = foreignKeys;
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
                mapColumn.Length = (ushort)column.Length.Value;
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