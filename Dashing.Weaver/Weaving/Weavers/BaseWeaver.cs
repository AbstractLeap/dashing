namespace Dashing.Weaver.Weaving.Weavers {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using Dashing.Logging;

    public abstract class BaseWeaver : ITaskLogHelper, IWeaver {
        private const string BackingFieldTemplate = "<{0}>k__BackingField";

        public ILog Logger { get; set; } = LogProvider.GetCurrentClassLogger();

        protected FieldDefinition GetBackingField(PropertyDefinition propertyDef) {
            // have a look for a field matching the standard format
            var fieldDef = propertyDef.DeclaringType.Fields.SingleOrDefault(f => f.Name == string.Format(BackingFieldTemplate, propertyDef.Name));
            if (fieldDef != null) {
                return fieldDef;
            }
            
            // look for stfld il in the setter
            var candidates =
                propertyDef.SetMethod.Body.Instructions.Where(
                    i =>
                        i.OpCode == OpCodes.Stfld && i.Operand is FieldDefinition
                                                  && ((FieldDefinition)i.Operand).FieldType.FullName == propertyDef.PropertyType.FullName).ToArray();
            if (candidates.Length == 1) {
                // they only store one thing in a field
                return (FieldDefinition)candidates.First().Operand;
            }
            else if (candidates.Count(i => i.Previous != null && i.Previous.OpCode == OpCodes.Ldarg_1) == 1) {
                // they only store one thing in a field by the previous instruction is to load the "value" on to the stack
                return (FieldDefinition)candidates.Single(i => i.Previous != null && i.Previous.OpCode == OpCodes.Ldarg_1).Operand;
            }

            // look for fields of this type loaded on to the stack
            candidates = propertyDef.GetMethod.Body.Instructions.Where(
                                            i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldDefinition
                                                 && ((FieldDefinition)i.Operand).FieldType.FullName == propertyDef.PropertyType.FullName)
                                        .ToArray();
            if (candidates.Length == 1) {
                return (FieldDefinition)candidates.First().Operand;
            }

            this.Logger.Error("Unable to determine backing field for property " + propertyDef.FullName);
            
            return null;
        }

        protected FieldDefinition GetField(TypeDefinition typeDefinition, string name) {
            var field = typeDefinition.Fields.SingleOrDefault(f => f.Name == name);
            if (field != null) {
                return field;
            }

            if (typeDefinition.BaseType.FullName == typeof(object).FullName) {
                this.Logger.Error("Unable to find Field " + name);

                return null;
            }

            return this.GetField(typeDefinition.BaseType.Resolve(), name);
        }

        protected PropertyDefinition GetProperty(TypeDefinition typeDef, string name) {
            var prop = typeDef.Properties.SingleOrDefault(p => p.Name == name);
            if (prop != null) {
                return prop;
            }

            if (typeDef.BaseType.FullName == typeof(object).FullName) {
                this.Logger.Error("Unable to find Property " + name);

                return null;
            }

            return this.GetProperty(typeDef.BaseType.Resolve(), name);
        }

        protected bool HasPropertyInInheritanceChain(TypeDefinition typeDefinition, string name) {
            if (typeDefinition.Properties.Any(p => p.Name == name)) {
                return true;
            }

            if (typeDefinition.BaseType.FullName == typeof(object).FullName) {
                return false;
            }

            return this.HasPropertyInInheritanceChain(typeDefinition.BaseType.Resolve(), name);
        }

        protected Stack<TypeDefinition> GetClassHierarchy(TypeDefinition typeDef) {
            var classHierarchy = new Stack<TypeDefinition>();
            var thisTypeDef = typeDef;
            do {
                classHierarchy.Push(thisTypeDef);
                thisTypeDef = thisTypeDef.BaseType.Resolve();
            }
            while (thisTypeDef.FullName != typeof(object).FullName);
            return classHierarchy;
        }

        protected static TypeReference MakeGenericType(TypeReference self, params TypeReference[] arguments) {
            if (self.GenericParameters.Count != arguments.Length) {
                throw new ArgumentException();
            }

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments) {
                instance.GenericArguments.Add(argument);
            }

            return instance;
        }

        protected static MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments) {
            var reference =
                new MethodReference(self.Name, self.ReturnType) {
                                                                    DeclaringType = MakeGenericType(self.DeclaringType, arguments),
                                                                    HasThis = self.HasThis,
                                                                    ExplicitThis = self.ExplicitThis,
                                                                    CallingConvention = self.CallingConvention
                                                                };

            foreach (var parameter in self.Parameters) {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var generic_parameter in self.GenericParameters) {
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
            }

            return reference;
        }

        protected void MakeNotDebuggerBrowsable(ModuleDefinition module, FieldDefinition field) {
            // TODO Figure out why cecil fails to write this correctly
#if !COREFX
            var debuggerBrowsableConstructor = module.ImportReference(typeof(DebuggerBrowsableAttribute).GetConstructors().First());
            var debuggerBrowsableAttr = new CustomAttribute(debuggerBrowsableConstructor);
            debuggerBrowsableAttr.ConstructorArguments.Add(
                new CustomAttributeArgument(
                    module.ImportReference(typeof(DebuggerBrowsableState)),
                    DebuggerBrowsableState.Never));
            field.CustomAttributes.Add(debuggerBrowsableAttr);
#endif
        }

        protected bool DoesNotUseObjectMethod(TypeDefinition typeDefinition, string methodName) {
            return typeDefinition.Methods.Any(m => m.Name == methodName)
                   || (typeDefinition.BaseType.FullName != typeof(object).FullName
                       && this.DoesNotUseObjectMethod(typeDefinition.BaseType.Resolve(), methodName));
        }

        protected void AddInterfaceToNonObjectAncestor(TypeDefinition typeDefinition, Type interfaceType) {
            if (typeDefinition.BaseType.FullName == typeof(object).FullName) {
                typeDefinition.Interfaces.Add(new InterfaceImplementation(typeDefinition.Module.ImportReference(interfaceType)));
            }
            else {
                this.AddInterfaceToNonObjectAncestor(typeDefinition.BaseType.Resolve(), interfaceType);
            }
        }

        protected bool IsBaseClass(TypeDefinition typeDefinition) {
            return typeDefinition.BaseType.FullName == typeof(object).FullName;
        }

        public abstract void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions);
    }
}