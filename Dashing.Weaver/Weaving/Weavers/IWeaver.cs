namespace Dashing.Weaver.Weaving.Weavers {
    using System.Collections.Generic;

    using Mono.Cecil;

    public interface IWeaver {
        void Weave(AssemblyDefinition assemblyDefinition, TypeDefinition typeDefinition, IEnumerable<ColumnDefinition> columnDefinitions);
    }
}