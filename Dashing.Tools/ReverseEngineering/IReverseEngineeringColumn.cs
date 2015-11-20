namespace Dashing.Tools.ReverseEngineering {
    using System;
    using System.Collections.Generic;

    internal interface IReverseEngineeringColumn {
        string ForeignKeyTableName { get; set; }

        IDictionary<string, Type> TypeMap { get; set; }
    }
}