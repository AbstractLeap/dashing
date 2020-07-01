namespace Dashing.Engine.DapperMapperGeneration {
    using System;

    using Dashing.Engine.DML;

    internal interface IMultiCollectionMapperGenerator {
        /// <summary>
        ///     Generates a multi collection mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapQueryTree"></param>
        /// <returns>The mapper, the list of types fetched, the list of collection types</returns>
        Tuple<Delegate, Type[], Type[]> GenerateMultiCollectionMapper<T>(QueryTree mapQueryTree);
    }
}