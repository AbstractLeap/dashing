namespace Dashing.Cli.Weaving.Weavers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class CollectionInstantiationWeaver : BaseWeaver {
        public override void Weave(
            TypeDefinition typeDef,
            AssemblyDefinition assemblyDefinition,
            MapDefinition mapDefinition,
            Dictionary<string, List<MapDefinition>> assemblyMapDefinitions,
            Dictionary<string, AssemblyDefinition> assemblyDefinitions) {
            var constructors = typeDef.GetConstructors().ToArray();
            foreach (var oneToManyColumnDefinition in mapDefinition.ColumnDefinitions.Where(c => c.Relationship == RelationshipType.OneToMany)) {
                var propDef = this.GetProperty(typeDef, oneToManyColumnDefinition.Name);
                if (propDef.SetMethod.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)) {
                    // auto prop - see if the prop set method is called in any of the constructors
                    if (!constructors.Any(c => c.Body.Instructions.Any(i => i.Operand != null && i.Operand.Equals(propDef.SetMethod)))) {
                        this.InstantiateCollection(typeDef, constructors, propDef);
                    }
                }
                else {
                    // not an auto prop
                    var backingField = this.GetBackingField(propDef);
                    if (
                        !constructors.Any(
                            c =>
                            c.Body.Instructions.Any(i => i.Operand != null && (i.Operand.Equals(propDef.SetMethod) || i.Operand.Equals(backingField))))) {
                        this.InstantiateCollection(typeDef, constructors, propDef);
                    }
                }
            }
        }

        private void InstantiateCollection(TypeDefinition typeDef, MethodDefinition[] constructors, PropertyDefinition propDef) {
            var constructor = constructors.First();
            if (constructors.Length > 1) {
                constructor = constructors.SingleOrDefault(s => !s.HasParameters && !s.IsStatic);
                if (constructor == null) {
                    this.Log.Error("Type " + typeDef.FullName + " does not have a parameterless constructor for instantiating collections in");
                }
            }

            var insertIdx = constructor.Body.Instructions.Count - 1;
            constructor.Body.Instructions.Insert(insertIdx++, Instruction.Create(OpCodes.Nop));
            constructor.Body.Instructions.Insert(insertIdx++, Instruction.Create(OpCodes.Ldarg_0));
            constructor.Body.Instructions.Insert(
                insertIdx++,
                Instruction.Create(
                    OpCodes.Newobj,
                    MakeGeneric(
                        typeDef.Module.Import(
                            typeDef.Module.Import(typeof(List<>))
                                   .MakeGenericInstanceType(propDef.PropertyType)
                                   .Resolve()
                                   .GetConstructors()
                                   .First(c => !c.HasParameters)),
                        ((GenericInstanceType)propDef.PropertyType).GenericArguments.First())));
            constructor.Body.Instructions.Insert(insertIdx, Instruction.Create(OpCodes.Call, propDef.SetMethod));
        }
    }
}