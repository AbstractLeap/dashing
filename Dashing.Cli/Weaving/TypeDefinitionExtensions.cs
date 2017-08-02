namespace Dashing.CodeGeneration.Weaving
{
    using System;

    public static class TypeDefinitionExtensions
    {
        public static bool ImplementsInterface(this TypeDefinition typeDefinition, Type interfaceType)
        {
            if (typeDefinition.Interfaces.Any(i => i.FullName == interfaceType.FullName))
            {
                return true;
            }

            if (typeDefinition.BaseType.FullName == typeof(object).FullName)
            {
                return false;
            }

            return typeDefinition.BaseType.Resolve().ImplementsInterface(interfaceType);
        }
    }
}