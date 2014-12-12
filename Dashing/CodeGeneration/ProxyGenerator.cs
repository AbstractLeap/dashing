namespace Dashing.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public class ProxyGenerator : IProxyGenerator {
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags (this usage is legal, see http://msdn.microsoft.com/en-us/library/system.codedom.memberattributes%28v=vs.110%29.aspx)
        private const MemberAttributes FinalPublic =
            MemberAttributes.Final | MemberAttributes.Public;

        public ProxyGeneratorResult GenerateProxies(
            CodeGeneratorConfig codeGeneratorConfig, 
            IDictionary<Type, IMap> mapDictionary) {
            var maps = mapDictionary.Values;
            var parallelMaps = maps.AsParallel();

            // create code doms for the proxy classes
            var trackingClasses =
                parallelMaps.Select(m => this.CreateTrackingClass(m, codeGeneratorConfig));
            var foreignKeyClasses =
                parallelMaps.Select(m => this.CreateFkClass(m, mapDictionary, codeGeneratorConfig));
            var updateClasses =
                parallelMaps.Select(
                    m => this.CreateUpdateClass(m, mapDictionary, codeGeneratorConfig));

            // extract metadata from maps
            var typeHierarchy =
                maps.Select(m => m.Type)
                    .Union(maps.SelectMany(m => m.Type.GetAncestorTypes()))
                    .ToArray();

            var namespaces =
                typeHierarchy.Select(m => m.Namespace)
                             .Distinct()
                             .Select(ns => new CodeNamespaceImport(ns));

            var references = typeHierarchy.Select(t => t.Assembly)
                                          .Distinct()
                                          .Select(a => a.Location);

            return new ProxyGeneratorResult {
                                                ProxyTypes =
                                                    trackingClasses.Concat(foreignKeyClasses)
                                                                   .Concat(updateClasses)
                                                                   .ToArray(), 
                                                NamespaceImports = namespaces.ToArray(), 
                                                ReferencedAssemblyLocations = references.ToArray()
                                            };
        }

        private CodeTypeDeclaration CreateUpdateClass(
            IMap map, 
            IDictionary<Type, IMap> maps, 
            CodeGeneratorConfig codeGeneratorConfig) {
            // public class Post_Update
            var updateClass =
                new CodeTypeDeclaration(map.Type.Name + codeGeneratorConfig.UpdateClassSuffix) {
                    IsClass = true,
                    TypeAttributes = TypeAttributes.Public
                };
            updateClass.BaseTypes.Add(map.Type);
            updateClass.BaseTypes.Add(typeof(IUpdateClass));

            // public IList<string> UpdatedProperties { get; set; }
            this.GenerateGetSetProperty(
                updateClass, 
                "UpdatedProperties", 
                typeof(IList<>).MakeGenericType(typeof(string)), 
                FinalPublic);

            // public bool IsConstructed { get; set; }
            this.GenerateGetSetProperty(updateClass, "IsConstructed", typeof(bool), FinalPublic);

            // public Post_Update() {
            //   this.IsConstructed = true;
            //   this.backingUpdatedProperties = new List<string>();
            // }
            var constructor = new CodeConstructor { Attributes = MemberAttributes.Public };
            constructor.Statements.Add(new CodeAssignStatement(CodeHelpers.ThisProperty("IsConstructed"), new CodePrimitiveExpression(true)));
            constructor.Statements.Add(new CodeAssignStatement(CodeHelpers.ThisProperty("UpdatedProperties"), new CodeObjectCreateExpression(typeof(List<>).MakeGenericType(typeof(string)))));
            updateClass.Members.Add(constructor);

            // public override IList<Comment> Comments { 
            //   get {
            //     return this.backingComments;
            //   }
            //   set {
            //     if (this.IsConstructed) {
            //       this.UpdatedProperties.Add("Comments");
            //     }
            //     this.backingComments = value;
            //   }
            // }
            foreach (var column in map.Columns.Where(c => !c.Value.IsIgnored)) {
                var prop = this.GenerateGetSetProperty(
                    updateClass, 
                    column.Key, 
                    column.Value.Type, 
                    MemberAttributes.Public | MemberAttributes.Override, 
                    true);

                var ifIsConstructed = CodeHelpers.ThisPropertyIsTrue("IsConstructed");

                var addColumnToUpdatedProperties =
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            CodeHelpers.ThisProperty("UpdatedProperties"), 
                            "Add", 
                            new CodePrimitiveExpression(column.Key)));

                prop.SetStatements.Insert(0, new CodeConditionStatement(ifIsConstructed, addColumnToUpdatedProperties));
            }

            return updateClass;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", 
            "SA1118:ParameterMustNotSpanMultipleLines", 
            Justification = "This is hard to read the StyleCop way")]
        private CodeTypeDeclaration CreateTrackingClass(
            IMap map, 
            CodeGeneratorConfig codeGeneratorConfig) {
            var trackingClass =
                new CodeTypeDeclaration(map.Type.Name + codeGeneratorConfig.TrackedClassSuffix);
            trackingClass.IsClass = true;
            trackingClass.TypeAttributes = TypeAttributes.Public;
            trackingClass.BaseTypes.Add(
                map.Type.Name + codeGeneratorConfig.ForeignKeyAccessClassSuffix);
            trackingClass.BaseTypes.Add(typeof(ITrackedEntity));

            // add in change tracking properties
            this.GenerateGetSetProperty(trackingClass, "IsTracking", typeof(bool), FinalPublic);
            this.GenerateGetSetProperty(
                trackingClass, 
                "DirtyProperties", 
                typeof(ISet<>).MakeGenericType(typeof(string)), 
                FinalPublic);
            this.GenerateGetSetProperty(
                trackingClass, 
                "OldValues", 
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)), 
                FinalPublic);
            this.GenerateGetSetProperty(
                trackingClass, 
                "NewValues", 
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)), 
                FinalPublic);
            this.GenerateGetSetProperty(
                trackingClass, 
                "AddedEntities", 
                typeof(IDictionary<,>).MakeGenericType(
                    typeof(string), 
                    typeof(IList<>).MakeGenericType(typeof(object))), 
                FinalPublic);
            this.GenerateGetSetProperty(
                trackingClass, 
                "DeletedEntities", 
                typeof(IDictionary<,>).MakeGenericType(
                    typeof(string), 
                    typeof(IList<>).MakeGenericType(typeof(object))), 
                FinalPublic);

            // add in a constructor to initialise collections
            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("DirtyProperties"), 
                    new CodeObjectCreateExpression(
                        typeof(HashSet<>).MakeGenericType(typeof(string)))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("OldValues"), 
                    new CodeObjectCreateExpression(
                        typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("NewValues"), 
                    new CodeObjectCreateExpression(
                        typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("AddedEntities"), 
                    new CodeObjectCreateExpression(
                        typeof(Dictionary<,>).MakeGenericType(
                            typeof(string), 
                            typeof(IList<>).MakeGenericType(typeof(object))))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("DeletedEntities"), 
                    new CodeObjectCreateExpression(
                        typeof(Dictionary<,>).MakeGenericType(
                            typeof(string), 
                            typeof(IList<>).MakeGenericType(typeof(object))))));

            // these constructor statements override the collection properties to use observable collections
            foreach (var collectionColumn in map.Columns.Where(c => c.Value.Type.IsCollection())) {
                if (
                    !collectionColumn.Value.Map.Type.GetProperty(collectionColumn.Value.Name)
                                     .GetGetMethod()
                                     .IsVirtual) {
                    // TODO: send a warning back to the programmer, did they mean to do this?
                    continue;
                }

                constructor.Statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), 
                                collectionColumn.Key), 
                            CodeBinaryOperatorType.IdentityEquality, 
                            new CodePrimitiveExpression(null)), 
                        new CodeStatement[] {
                                                new CodeAssignStatement(
                                                    new CodePropertyReferenceExpression(
                                                    new CodeThisReferenceExpression(), 
                                                    collectionColumn.Key), 
                                                    new CodeObjectCreateExpression(
                                                    "Dashing.CodeGeneration.TrackingCollection<" + trackingClass.Name
                                                    + "," + collectionColumn.Value.Type.GenericTypeArguments.First()
                                                    + ">", 
                                                    new CodeThisReferenceExpression(), 
                                                    new CodePrimitiveExpression(collectionColumn.Key)))
                                            }, 
                        new CodeStatement[] {
                                                new CodeAssignStatement(
                                                    new CodePropertyReferenceExpression(
                                                    new CodeThisReferenceExpression(), 
                                                    collectionColumn.Key), 
                                                    new CodeObjectCreateExpression(
                                                    "Dashing.CodeGeneration.TrackingCollection<" + trackingClass.Name
                                                    + "," + collectionColumn.Value.Type.GenericTypeArguments.First()
                                                    + ">", 
                                                    new CodeThisReferenceExpression(), 
                                                    new CodePrimitiveExpression(collectionColumn.Key), 
                                                    new CodePropertyReferenceExpression(
                                                    new CodeThisReferenceExpression(), 
                                                    collectionColumn.Key)))
                                            }));
            }

            // override value type properties to perform dirty checking
            foreach (
                var valueTypeColumn in
                    map.Columns.Where(c => !c.Value.Type.IsCollection() && !c.Value.IsIgnored)) {
                if (
                    !valueTypeColumn.Value.Map.Type.GetProperty(valueTypeColumn.Value.Name)
                                    .GetGetMethod()
                                    .IsVirtual) {
                    // TODO: send a warning back to the programmer, did they mean to do this?
                    continue;
                }

                var prop = this.GenerateGetSetProperty(
                    trackingClass, 
                    valueTypeColumn.Key, 
                    valueTypeColumn.Value.Type, 
                    MemberAttributes.Public | MemberAttributes.Override, 
                    true);

                // override the setter
                // if isTracking && !this.DirtyProperties.ContainsKey(prop) add to dirty props and add oldvalue
                bool propertyCanBeNull = valueTypeColumn.Value.Type.IsNullable()
                                         || !valueTypeColumn.Value.Type.IsValueType;
                var changeCheck = new CodeBinaryOperatorExpression();
                if (!propertyCanBeNull) {
                    // can't be null so just check values
                    changeCheck.Left =
                        new CodeMethodInvokeExpression(
                            CodeHelpers.BaseProperty(valueTypeColumn.Key), 
                            "Equals", 
                            new CodePropertySetValueReferenceExpression());
                    changeCheck.Operator = CodeBinaryOperatorType.IdentityEquality;
                    changeCheck.Right = new CodePrimitiveExpression(false);
                }
                else {
                    // can be null, need to be careful of null reference exceptions
                    changeCheck.Left =
                        new CodeBinaryOperatorExpression(
                            CodeHelpers.BasePropertyIsNull(valueTypeColumn.Key), 
                            CodeBinaryOperatorType.BooleanAnd, 
                            new CodeBinaryOperatorExpression(
                                new CodePropertySetValueReferenceExpression(), 
                                CodeBinaryOperatorType.IdentityInequality, 
                                new CodePrimitiveExpression(null)));
                    changeCheck.Operator = CodeBinaryOperatorType.BooleanOr;
                    changeCheck.Right =
                        new CodeBinaryOperatorExpression(
                            CodeHelpers.BasePropertyIsNotNull(valueTypeColumn.Key), 
                            CodeBinaryOperatorType.BooleanAnd, 
                            new CodeBinaryOperatorExpression(
                                new CodeMethodInvokeExpression(
                                    CodeHelpers.BaseProperty(valueTypeColumn.Key), 
                                    "Equals", 
                                    new CodePropertySetValueReferenceExpression()), 
                                CodeBinaryOperatorType.IdentityEquality, 
                                new CodePrimitiveExpression(false)));
                }

                prop.SetStatements.Insert(
                    0, 
                    new CodeConditionStatement(
                        CodeHelpers.ThisPropertyIsTrue("IsTracking"), 
                        new CodeStatement[] {
                                                new CodeConditionStatement(
                                                    new CodeBinaryOperatorExpression(
                                                    new CodeBinaryOperatorExpression(
                                                    new CodeMethodInvokeExpression(
                                                    CodeHelpers.ThisProperty("DirtyProperties"), 
                                                    "Contains", 
                                                    new CodePrimitiveExpression(prop.Name)), 
                                                    CodeBinaryOperatorType.IdentityEquality, 
                                                    new CodePrimitiveExpression(false)), 
                                                    CodeBinaryOperatorType.BooleanAnd, 
                                                    changeCheck), 
                                                    new CodeStatement[] {
                                                                            new CodeExpressionStatement(
                                                                                new CodeMethodInvokeExpression(
                                                                                CodeHelpers.ThisProperty("DirtyProperties"), 
                                                                                "Add", 
                                                                                new CodePrimitiveExpression(prop.Name))), 
                                                                            new CodeAssignStatement(
                                                                                new CodeIndexerExpression(
                                                                                CodeHelpers.ThisProperty("OldValues"), 
                                                                                new CodePrimitiveExpression(prop.Name)), 
                                                                                new CodePropertySetValueReferenceExpression())
                                                                        }), 
                                                new CodeAssignStatement(
                                                    new CodeIndexerExpression(
                                                    CodeHelpers.ThisProperty("NewValues"), 
                                                    new CodePrimitiveExpression(prop.Name)), 
                                                    new CodePropertySetValueReferenceExpression())
                                            }));
            }

            trackingClass.Members.Add(constructor);

            return trackingClass;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", 
            "SA1118:ParameterMustNotSpanMultipleLines", 
            Justification = "This is hard to read the StyleCop way")]
        private CodeTypeDeclaration CreateFkClass(
            IMap map, 
            IDictionary<Type, IMap> maps, 
            CodeGeneratorConfig codeGeneratorConfig) {
            // generate the foreign key access class based on the original class
            var foreignKeyClass =
                new CodeTypeDeclaration(
                    map.Type.Name + codeGeneratorConfig.ForeignKeyAccessClassSuffix);
            foreignKeyClass.IsClass = true;
            foreignKeyClass.TypeAttributes = TypeAttributes.Public;
            foreignKeyClass.BaseTypes.Add(map.Type);

            foreach (
                var column in
                    map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne || c.Value.Relationship == RelationshipType.OneToOne)) {
                if (!column.Value.Map.Type.GetProperty(column.Value.Name).GetGetMethod().IsVirtual) {
                    // TODO: send a warning back to the programmer, did they mean to do this?
                    continue;
                }

                // create a backing property for storing the FK
                var backingType = column.Value.DbType.GetCLRType();
                if (backingType.IsValueType) {
                    backingType = typeof(Nullable<>).MakeGenericType(backingType);
                }

                var foreignKeyBackingProperty = this.GenerateGetSetProperty(
                    foreignKeyClass, 
                    column.Value.DbName, 
                    backingType, 
                    FinalPublic);

                // create a backing field for storing the related entity
                var backingField = new CodeMemberField(
                    column.Value.Type, 
                    column.Value.Name + codeGeneratorConfig.ForeignKeyAccessEntityFieldSuffix);
                foreignKeyClass.Members.Add(backingField);

                // override the property getter and setter to use the backingfield
                var property = new CodeMemberProperty();
                property.Name = column.Value.Name;
                property.Type = new CodeTypeReference(column.Value.Type);
                property.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                property.GetStatements.Add(
                    new CodeConditionStatement(
                        //// if backingField != null or Fk backing field is null return
                        new CodeBinaryOperatorExpression(
                            CodeHelpers.ThisFieldIsNotNull(backingField.Name), 
                            CodeBinaryOperatorType.BooleanOr, 
                            CodeHelpers.ThisPropertyIsNull(foreignKeyBackingProperty.Name)), 
                        new CodeStatement[] {
                                                // true
                                                new CodeMethodReturnStatement(
                                                    CodeHelpers.ThisField(backingField.Name))
                                            }, 
                        new CodeStatement[] {
                                                // false, return new object with foreign key set
                                                new CodeVariableDeclarationStatement(
                                                    column.Value.Type, 
                                                    "val", 
                                                    new CodeObjectCreateExpression(
                                                    column.Value.Type)), 
                                                new CodeAssignStatement(
                                                    new CodeFieldReferenceExpression(
                                                    new CodeVariableReferenceExpression("val"), 
                                                    maps[column.Value.Type].PrimaryKey.Name), 
                                                    new CodePropertyReferenceExpression(
                                                    CodeHelpers.ThisProperty(
                                                        foreignKeyBackingProperty.Name), 
                                                    "Value")), 
                                                new CodeAssignStatement(
                                                    CodeHelpers.ThisField(backingField.Name), 
                                                    new CodeVariableReferenceExpression("val")), 
                                                new CodeMethodReturnStatement(
                                                    new CodeVariableReferenceExpression("val"))
                                            }));
                property.SetStatements.Add(
                    new CodeAssignStatement(
                        CodeHelpers.ThisField(backingField.Name), 
                        new CodePropertySetValueReferenceExpression()));
                foreignKeyClass.Members.Add(property);
            }

            return foreignKeyClass;
        }

        private CodeMemberProperty GenerateGetSetProperty(
            CodeTypeDeclaration owningClass, 
            string name, 
            Type type, 
            MemberAttributes attributes, 
            bool useBaseProperty = false) {
            // generate the property
            var prop = new CodeMemberProperty();
            prop.Name = name;
            prop.Type = new CodeTypeReference(type);
            prop.Attributes = attributes;

            if (useBaseProperty) {
                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(CodeHelpers.BaseProperty(name)));
                prop.SetStatements.Add(
                    new CodeAssignStatement(
                        CodeHelpers.BaseProperty(name), 
                        new CodePropertySetValueReferenceExpression()));
            }
            else {
                // generate the backing field for this property
                var backingField = new CodeMemberField();
                backingField.Name = "backing" + name;
                backingField.Type = new CodeTypeReference(type);
                owningClass.Members.Add(backingField);

                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(CodeHelpers.ThisField(backingField.Name)));
                prop.SetStatements.Add(
                    new CodeAssignStatement(
                        CodeHelpers.ThisField(backingField.Name), 
                        new CodePropertySetValueReferenceExpression()));
            }

            owningClass.Members.Add(prop);

            return prop;
        }
    }
}