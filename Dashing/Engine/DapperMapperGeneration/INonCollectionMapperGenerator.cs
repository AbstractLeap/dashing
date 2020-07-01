namespace Dashing.Engine.DapperMapperGeneration {
    using System;

    using Dashing.Engine.DML;

    internal interface INonCollectionMapperGenerator {
        Tuple<Delegate, Type[]> GenerateNonCollectionMapper<T>(QueryTree mapQueryTree);
    }
}