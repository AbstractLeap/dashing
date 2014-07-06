namespace Dashing.Engine.DapperMapperGeneration {
    using System;

    internal interface IDapperMapperGenerator {
        /// <summary>
        ///     Generates a Func for the passed in fetchTree
        /// </summary>
        /// <typeparam name="T">The base type of the tree</typeparam>
        /// <param name="fetchTree">The fetch tree to generate the mapper for</param>
        /// <returns>A factory for generating mappers</returns>
        Delegate GenerateCollectionMapper<T>(FetchNode fetchTree, bool isTracked);

        Delegate GenerateNonCollectionMapper<T>(FetchNode fetchTree, bool isTracked);
    }
}