using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Extensions;

namespace TopHat.CodeGeneration
{
    internal class CodeGenerator : ICodeGenerator
    {
        private CodeCompileUnit compileUnit;
        private CodeNamespace codeNamespace;

        private HashSet<string> referencedAssemblies;

        public CodeGenerator()
        {
        }

        public void Generate(IConfiguration configuration, CodeGeneratorConfig generatorConfig)
        {
            this.Init(configuration, generatorConfig);

            Parallel.ForEach(configuration.Maps, i => { this.Generate(configuration, generatorConfig, i); });
            // generate the code
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            if (generatorConfig.GenerateSource)
            {
                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.BracingStyle = "C";
                using (System.IO.StreamWriter sourceWriter = new System.IO.StreamWriter("D:\\source.cs"))
                {
                    provider.GenerateCodeFromCompileUnit(this.compileUnit, sourceWriter, options);
                }
            }

            if (generatorConfig.GenerateAssembly)
            {
                var parameters = new CompilerParameters(this.referencedAssemblies.ToArray(), generatorConfig.Namespace + ".dll", true);
                var results = provider.CompileAssemblyFromDom(parameters, this.compileUnit);
            }
        }

        private void Generate(IConfiguration configuration, CodeGeneratorConfig generatorConfig, IMap map)
        {
            // add this assembly
            this.referencedAssemblies.Add(map.Type.Assembly.GetName().Name + ".dll");

            // add the namespace of this type as a using statement
            this.codeNamespace.Imports.Add(new CodeNamespaceImport(map.Type.Namespace));

            // create the FK access class
            this.CreateFKClass(configuration, generatorConfig, map);

            // create the tracking class
            this.CreateTrackingClass(configuration, generatorConfig, map);
        }

        private void CreateTrackingClass(IConfiguration configuration, CodeGeneratorConfig generatorConfig, IMap map)
        {
            var trackingClass = new CodeTypeDeclaration(map.Type.Name + generatorConfig.TrackedClassSuffix);
            trackingClass.IsClass = true;
            trackingClass.TypeAttributes = TypeAttributes.Public;
            trackingClass.BaseTypes.Add(map.Type.Name + generatorConfig.ForeignKeyAccessClassSuffix);
            trackingClass.BaseTypes.Add(typeof(ITrackedEntity));

            // add in change tracking properties
            this.GenerateGetSetProperty(trackingClass, "IsTracking", typeof(bool), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(trackingClass, "DirtyProperties", typeof(ISet<>).MakeGenericType(typeof(string)), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(trackingClass, "OldValues", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(trackingClass, "AddedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(trackingClass, "DeletedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))), MemberAttributes.Public | MemberAttributes.Final);

            // add in a constructor to initialise collections
            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "DirtyProperties"),
                    new CodeObjectCreateExpression(typeof(HashSet<>).MakeGenericType(typeof(string)))
                )
            );
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "OldValues"),
                    new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)))
                )
            );
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AddedEntities"),
                    new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))))
                )
            );
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "DeletedEntities"),
                    new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))))
                )
            );

            // these constructor statements override the collection properties to use observable collections
            foreach (var collectionColumn in map.Columns.Where(c => c.Value.Type.IsCollection()))
            {
                constructor.Statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(null)
                        ),
                        new CodeStatement[] {
                            new CodeAssignStatement(
                                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                                new CodeObjectCreateExpression("TopHat.CodeGeneration.TrackingCollection<" + trackingClass.Name + "," + collectionColumn.Value.Type.GenericTypeArguments.First() + ">", new CodeThisReferenceExpression(), new CodePrimitiveExpression(collectionColumn.Key))
                            )
                        },
                        new CodeStatement[] {
                            new CodeAssignStatement(
                                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                                new CodeObjectCreateExpression("TopHat.CodeGeneration.TrackingCollection<" + trackingClass.Name + "," + collectionColumn.Value.Type.GenericTypeArguments.First() + ">", new CodeThisReferenceExpression(), new CodePrimitiveExpression(collectionColumn.Key), new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key))
                            )
                        }
                    )
                );
            }

            // override value type properties to perform dirty checking
            foreach (var valueTypeColumn in map.Columns.Where(c => !c.Value.Type.IsCollection() && !c.Value.Ignore))
            {
                var prop = this.GenerateGetSetProperty(trackingClass, valueTypeColumn.Key, valueTypeColumn.Value.Type, MemberAttributes.Public | MemberAttributes.Override);
                // override the setter
                // if isTracking && !this.DirtyProperties.ContainsKey(prop) add to dirty props and add oldvalue
                bool propertyCanBeNull = valueTypeColumn.Value.Type.IsNullable() || !valueTypeColumn.Value.Type.IsValueType;
                var changeCheck = new CodeBinaryOperatorExpression();
                if (!propertyCanBeNull)
                {
                    // can't be null so just check values
                    changeCheck.Left = new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "backing" + valueTypeColumn.Key), "Equals", new CodePropertySetValueReferenceExpression());
                    changeCheck.Operator = CodeBinaryOperatorType.IdentityEquality;
                    changeCheck.Right = new CodePrimitiveExpression(false);
                }
                else
                {
                    // can be null, need to be careful of null reference exceptions
                    changeCheck.Left = new CodeBinaryOperatorExpression(
                        CodeHelpers.ThisFieldIsNull("backing" + valueTypeColumn.Key),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodePropertySetValueReferenceExpression(),
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)
                        )
                    );
                    changeCheck.Operator = CodeBinaryOperatorType.BooleanOr;
                    changeCheck.Right = new CodeBinaryOperatorExpression(
                        CodeHelpers.ThisFieldIsNotNull("backing" + valueTypeColumn.Key),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "backing" + valueTypeColumn.Key), "Equals", new CodePropertySetValueReferenceExpression()),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(false)
                        )
                    );
                }

                prop.SetStatements.Insert(0, new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        CodeHelpers.ThisPropertyIsTrue("IsTracking"),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodeBinaryOperatorExpression(
                                new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "DirtyProperties"), "Contains", new CodePrimitiveExpression(prop.Name)),
                                CodeBinaryOperatorType.IdentityEquality,
                                new CodePrimitiveExpression(false)
                            ),
                            CodeBinaryOperatorType.BooleanAnd,
                            changeCheck
                        )
                    ),
                    new CodeStatement[] {
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "DirtyProperties"), "Add", new CodePrimitiveExpression(prop.Name))),
                        new CodeAssignStatement(
                            new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "OldValues"), new CodePrimitiveExpression(prop.Name)),
                            new CodePropertySetValueReferenceExpression()
                        )
                    }
                ));
            }

            trackingClass.Members.Add(constructor);

            this.codeNamespace.Types.Add(trackingClass);
        }

        private void CreateFKClass(IConfiguration configuration, CodeGeneratorConfig generatorConfig, IMap map)
        {
            // generate the foreign key access class based on the original class
            var fkClass = new CodeTypeDeclaration(map.Type.Name + generatorConfig.ForeignKeyAccessClassSuffix);
            fkClass.IsClass = true;
            fkClass.TypeAttributes = TypeAttributes.Public;
            fkClass.BaseTypes.Add(map.Type);

            foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne))
            {
                // create a backing field for storing the FK
                var fkBackingField = new CodeMemberField(column.Value.DbType.GetCLRType(), column.Value.DbName); // TODO add alias to column names (as spaces will break this)
                fkBackingField.Attributes = MemberAttributes.Public;
                fkClass.Members.Add(fkBackingField);

                // create a backing field for storing the related entity
                var backingField = new CodeMemberField(column.Value.Type, column.Value.Name + generatorConfig.ForeignKeyAccessEntityFieldSuffix);
                fkClass.Members.Add(backingField);

                // override the property getter and setter to use the backingfield
                var property = new CodeMemberProperty();
                property.Name = column.Value.Name;
                property.Type = new CodeTypeReference(column.Value.Type);
                property.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                property.GetStatements.Add(new CodeConditionStatement(
                    // if backingField != null
                        CodeHelpers.ThisFieldIsNotNull(backingField.Name),
                    new CodeStatement[] { // true
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name)),
                    },
                        new CodeStatement[] {  // false, return new object with foreign key set
                            new CodeVariableDeclarationStatement(column.Value.Type, "val", new CodeObjectCreateExpression(column.Value.Type)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("val"), configuration.Maps.SingleOrDefault(m => m.Type == column.Value.Type).PrimaryKey), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fkBackingField.Name)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name), new CodeVariableReferenceExpression("val")),
                            new CodeMethodReturnStatement(new CodeVariableReferenceExpression("val"))
                        }
                    ));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name), new CodePropertySetValueReferenceExpression()));
                fkClass.Members.Add(property);
            }

            this.codeNamespace.Types.Add(fkClass);
        }

        private CodeMemberProperty GenerateGetSetProperty(CodeTypeDeclaration owningClass, string name, Type type, MemberAttributes attributes)
        {
            // generate the backing field for this property
            var backingField = new CodeMemberField();
            backingField.Name = "backing" + name;
            backingField.Type = new CodeTypeReference(type);
            owningClass.Members.Add(backingField);

            // generate the property
            var prop = new CodeMemberProperty();
            prop.Name = name;
            prop.Type = new CodeTypeReference(type);
            prop.Attributes = attributes;

            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name)
                )
            );
            prop.SetStatements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name),
                    new CodePropertySetValueReferenceExpression()
                )
            );
            owningClass.Members.Add(prop);

            return prop;
        }

        private void Init(IConfiguration configuration, CodeGeneratorConfig generatorConfig)
        {
            this.referencedAssemblies = new HashSet<string>();
            this.compileUnit = new CodeCompileUnit();
            this.codeNamespace = new CodeNamespace(generatorConfig.Namespace);
            this.compileUnit.Namespaces.Add(this.codeNamespace);

            // add standard usings
            this.codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            this.codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

            // add standard dll references
            this.referencedAssemblies.Add("System.dll");
            this.referencedAssemblies.Add("System.Core.dll");
            this.referencedAssemblies.Add("TopHat.dll");
        }
    }
}