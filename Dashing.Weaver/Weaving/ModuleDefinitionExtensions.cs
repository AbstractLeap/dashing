namespace Dashing.Weaver.Weaving {
    using System.Linq;

    using Mono.Cecil;

    public static class ModuleDefinitionExtensions {
        public static TypeDefinition GetTypeDefFromFullName(this ModuleDefinition moduleDefinition, string typeFullName) {
            TypeDefinition typeDef;
            if (typeFullName.Contains('+')) {
                var types = typeFullName.Split('+');
                typeDef = moduleDefinition.Types.Single(t => t.FullName == types.First());
                for (var i = 1; i < types.Length; i++) {
                    typeDef = typeDef.NestedTypes.Single(t => t.Name == types.ElementAt(i));
                }
            }
            else {
                typeDef = moduleDefinition.Types.Single(t => t.FullName == typeFullName);
            }

            return typeDef;
        }
    }
}