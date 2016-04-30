namespace Dashing.Console.Weaving.Weavers {
    using System.Collections.Generic;

    using Mono.Cecil;

    public interface IWeaver {
        int Precedence { get; }

        void Weave(
            TypeDefinition typeDef,
            AssemblyDefinition assemblyDefinition,
            MapDefinition mapDefinition,
            Dictionary<string, List<MapDefinition>> assemblyMapDefinitions,
            Dictionary<string, AssemblyDefinition> assemblyDefinitions);
    }
}