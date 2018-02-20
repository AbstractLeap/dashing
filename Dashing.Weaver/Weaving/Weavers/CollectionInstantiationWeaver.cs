namespace Dashing.Weaver.Weaving.Weavers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class CollectionInstantiationWeaver : BaseWeaver {
        public override void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions) {
            var constructors = typeDefinition.GetConstructors().ToArray();
            foreach (var oneToManyColumnDefinition in columnDefinitions.Where(c => c.Relationship == RelationshipType.OneToMany)) {
                var propDef = this.GetProperty(typeDefinition, oneToManyColumnDefinition.Name);
                if (propDef.SetMethod.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)) {
                    // auto prop - see if the prop set method is called in any of the constructors
                    if (!constructors.Any(c => c.Body.Instructions.Any(i => i.Operand != null && i.Operand.Equals(propDef.SetMethod)))) {
                        this.InstantiateCollection(typeDefinition, constructors, propDef);
                    }
                }
                else {
                    // not an auto prop
                    var backingField = this.GetBackingField(propDef);
                    if (!constructors.Any(
                            c => c.Body.Instructions.Any(
                                i => i.Operand != null && (i.Operand.Equals(propDef.SetMethod) || i.Operand.Equals(backingField))))) {
                        this.InstantiateCollection(typeDefinition, constructors, propDef);
                    }
                }
            }
        }

        private void InstantiateCollection(TypeDefinition typeDef, MethodDefinition[] constructors, PropertyDefinition propDef) {
            var constructor = constructors.First();
            if (constructors.Length > 1) {
                constructor = constructors.SingleOrDefault(s => !s.HasParameters);
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
                        typeDef.Module.ImportReference(
                            typeDef.Module.ImportReference(typeof(List<>))
                                   .MakeGenericInstanceType(propDef.PropertyType)
                                   .Resolve()
                                   .GetConstructors()
                                   .First(c => !c.HasParameters)),
                        ((GenericInstanceType)propDef.PropertyType).GenericArguments.First())));
            constructor.Body.Instructions.Insert(insertIdx, Instruction.Create(OpCodes.Call, propDef.SetMethod));
        }
    }
}