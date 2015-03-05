namespace Dashing.Engine.DapperMapperGeneration {
    using System;

    using Dashing.Engine.DML;

    internal interface IDapperMapperGenerator {
        /// <summary>
        ///     Generates a Func for the passed in fetchTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="fetchTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        Tuple<Delegate, Type[]> GenerateCollectionMapper<T>(FetchNode fetchTree, bool isTracked);

        Tuple<Delegate, Type[]> GenerateNonCollectionMapper<T>(FetchNode fetchTree, bool isTracked);

        /// <summary>
        /// Generates a multi collection mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fetchTree"></param>
        /// <param name="isTracked"></param>
        /// <returns>The mapper, the list of types fetched, the list of collection types</returns>
        Tuple<Delegate, Type[], Type[]> GenerateMultiCollectionMapper<T>(FetchNode fetchTree, bool isTracked);
    }
}