using System;
using System.Collections.Generic;

namespace TopHat.Configuration {
	public class Map {
		public Map(Type type) {
			if (type == null) throw new ArgumentNullException("type");

			Type = type;
			Columns = new Dictionary<string, Column>();
			Indexes = new List<IEnumerable<string>>();
		}

		public Type Type { get; private set; }

		public string Table { get; set; }

		public string Schema { get; set; }

		public string PrimaryKey { get; set; }

		public bool IsPrimaryKeyDatabaseGenerated { get; set; }

		public IDictionary<string, Column> Columns { get; set; }

		public IEnumerable<IEnumerable<string>> Indexes { get; set; }

		/*
		 * mark doing optimization
		public string SqlSelectByPrimaryKey { get; set; }

		public string SqlSelectByPrimaryKeyIncludeAllColumns { get; set; }

		public string SqlInsert { get; set; }

		public string SqlDeleteByPrimaryKey { get; set; }
		 * */
	}

	public class Map<T> : Map {
		public Map()
			: base(typeof (T)) {}

		/// <remarks>Highly inelegant wrapping of all the members, but probably quite performant</remarks>
		public static Map<T> From(Map map) {
			if (typeof (T) != map.Type) throw new ArgumentException("The argument does not represent a map of the correct generic type");

			return new Map<T> {
				Table = map.Table,
				Schema = map.Schema,
				PrimaryKey = map.PrimaryKey,
				IsPrimaryKeyDatabaseGenerated = map.IsPrimaryKeyDatabaseGenerated,
				Columns = map.Columns,
				Indexes = map.Indexes,
				/*
				 * mark doing optimization
				SqlSelectByPrimaryKey = map.SqlSelectByPrimaryKey,
				SqlSelectByPrimaryKeyIncludeAllColumns = map.SqlSelectByPrimaryKeyIncludeAllColumns,
				SqlInsert = map.SqlInsert,
				SqlDeleteByPrimaryKey = map.SqlDeleteByPrimaryKey
				 * */
			};
		}
	}
}