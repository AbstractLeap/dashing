namespace Dashing.Engine.DapperMapperGeneration {
    using System;

    using Dashing.Engine.DML;

    internal interface ISingleCollectionMapperGenerator {
        /// <summary>
        ///     Generates a Func for the passed in mapQueryTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="mapQueryTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        Tuple<Delegate, Type[]> GenerateCollectionMapper<T>(QueryTree mapQueryTree);
    }
}