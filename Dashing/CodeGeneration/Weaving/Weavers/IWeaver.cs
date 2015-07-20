namespace Dashing.CodeGeneration.Weaving.Weavers {
    using System.Collections.Generic;

    using Mono.Cecil;

    public interface IWeaver {
        void Weave(
            TypeDefinition typeDef,
            AssemblyDefinition assemblyDefinition,
            MapDefinition mapDefinition,
            Dictionary<string, List<MapDefinition>> assemblyMapDefinitions,
            Dictionary<string, AssemblyDefinition> assemblyDefinitions);
    }
}