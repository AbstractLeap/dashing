namespace Dashing.Tools.ReverseEngineering {
    using System;
    using System.Collections.Generic;

    using Dashing.Tools.SchemaReading;

    internal interface IReverseEngineeringColumn {
        string ForeignKeyTableName { get; set; }

        IDictionary<string, Type> TypeMap { get; set; }

        ColumnDto ColumnSpecification { get; set; }
    }
}