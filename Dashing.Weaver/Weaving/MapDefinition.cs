
namespace Dashing.Weaver.Weaving {
    using System.Collections.Generic;

    public class MapDefinition {
        public string AssemblyPath { get; set; }

        public string TypeFullName { get; set; }

        public IList<ColumnDefinition> ColumnDefinitions { get; set; }
    }
}