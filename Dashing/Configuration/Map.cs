namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Map : IMap {
        private MethodInfo nonGenericPrimaryKeyGetter;

        private readonly object nonGenericPrimaryKeyGetterLock = new object();

        public Map(Type type) {
            this.Type = type;
            this.Columns = new Dictionary<string, IColumn>();

            //// this.Indexes = new List<IEnumerable<string>>();
        }

        public IConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        ///     Gets or sets the table.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        ///     Gets or sets the schema.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        public IColumn PrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        public IDictionary<string, IColumn> Columns { get; set; }

        public object GetPrimaryKeyValue(object entity) {
            if (this.nonGenericPrimaryKeyGetter == null) {
                lock (this.nonGenericPrimaryKeyGetterLock) {
                    if (this.nonGenericPrimaryKeyGetter == null) {
                        this.nonGenericPrimaryKeyGetter = typeof(Map<>).MakeGenericType(this.Type)
                                                                       .GetMethods()
                                                                       .First(
                                                                           m => m.Name == "GetPrimaryKeyValue" && m.GetParameters()
                                                                                                                   .Any(p => p.ParameterType == this.Type));
                    }
                }
            }

            return this.nonGenericPrimaryKeyGetter.Invoke(this, new[] { entity });
        }

        //// commented out until we get basic stuff working
        ///// <summary>
        /////   Gets or sets the indexes.
        ///// </summary>
        //// public IEnumerable<IEnumerable<string>> Indexes { get; set; }
    }
}