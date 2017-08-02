namespace Dashing.ReverseEngineering {
    using System;

    using Dashing.Configuration;

    internal interface IReverseEngineeringConfiguration : IConfiguration {
        void AddMap(Type type, IMap map);
    }
}