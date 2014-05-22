using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration;
using TopHat.Extensions;

namespace TopHat.CodeGenerator
{
    internal class CodeGenerator : ICodeGenerator
    {
        private CodeCompileUnit compileUnit;
        private CodeNamespace codeNamespace;

        public const string Namespace = "TopHat.Generated";

        public const string ForeignKeyAccessClassSuffix = "_FK";

        public const string ForeignKeyAccessFieldSuffix = "_FKField";

        public const string ForeignKeyAccessEntityFieldSuffix = "_FKEntity";

        public CodeGenerator()
        {
            this.compileUnit = new CodeCompileUnit();
            this.codeNamespace = new CodeNamespace(Namespace);
            this.compileUnit.Namespaces.Add(this.codeNamespace);

            // add standard usings
            this.codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            this.codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
        }

        public void Generate(IConfiguration configuration, CodeGeneratorConfig generatorConfig)
        {
            Parallel.ForEach(configuration.Maps, i => { this.Generate(generatorConfig, i); });
            // generate the code
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (System.IO.StreamWriter sourceWriter = new System.IO.StreamWriter("D:\\source.cs"))
            {
                provider.GenerateCodeFromCompileUnit(this.compileUnit, sourceWriter, options);
            }
        }

        private void Generate(CodeGeneratorConfig generatorConfig, IMap map)
        {
            // add the namespace of this type as a using statement
            this.codeNamespace.Imports.Add(new CodeNamespaceImport(map.Type.Namespace));

            // generate the foreign key access class based on the original class
            var fkClass = new CodeTypeDeclaration(map.Type.Name + ForeignKeyAccessClassSuffix);
            fkClass.IsClass = true;
            fkClass.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
            fkClass.BaseTypes.Add(map.Type);

            foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne))
            {
                // create a backing field for storing the FK
                var fkBackingField = new CodeMemberField(column.Value.DbType.GetCLRType(), column.Value.Name + ForeignKeyAccessFieldSuffix);
                fkBackingField.Attributes = MemberAttributes.Public;
                fkClass.Members.Add(fkBackingField);

                // create a backing field for storing the related entity
                var backingField = new CodeMemberField(column.Value.Type, column.Value.Name + ForeignKeyAccessEntityFieldSuffix);
                fkClass.Members.Add(backingField);

                // override the property getter and setter to use the backingfield
                var property = new CodeMemberProperty();
                property.Name = column.Value.Name;
                property.Type = new CodeTypeReference(column.Value.Type);
                property.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                property.GetStatements.Add(new CodeConditionStatement(
                    // if backingField != null
                    new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                    new CodeStatement[] { // true
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name)),
                    },
                        new CodeStatement[] {  // false, return new object with foreign key set
                            new CodeVariableDeclarationStatement(column.Value.Type, "val", new CodeObjectCreateExpression(column.Value.Type)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("val"), map.PrimaryKey), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fkBackingField.Name)),
                            new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name), new CodeVariableReferenceExpression("val")),
                            new CodeMethodReturnStatement(new CodeVariableReferenceExpression("val"))
                        }
                    ));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name), new CodePropertySetValueReferenceExpression()));
                fkClass.Members.Add(property);
            }

            this.codeNamespace.Types.Add(fkClass);
        }
    }
}