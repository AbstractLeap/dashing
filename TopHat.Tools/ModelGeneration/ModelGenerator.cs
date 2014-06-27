using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TopHat.Configuration;
using System.CodeDom;
using DatabaseSchemaReader.DataSchema;

namespace TopHat.Tools.ModelGeneration
{
    public class ModelGenerator : IModelGenerator
    {
        private IConvention convention;

        public ModelGenerator() : this(new DefaultConvention()) { }

        public ModelGenerator(IConvention convention)
        {
            this.convention = convention;
        }

        public IEnumerable<string> GenerateFiles(IConfiguration configuration, DatabaseSchema schema, string domainNamespace)
        {
            // note that we're just doing string building here
            // simple POCOs and CodeDom does not support auto-properties
            // and frankly I wouldn't want these things to have backing fields in the source code
            var result = new List<string>();

            // iterate over the configuration generating classes
            foreach (var map in configuration.Maps)
            {
                this.GenerateClass(result, map, schema, domainNamespace);
            }

            return result;
        }

        private void GenerateClass(IList<string> result, IMap map, DatabaseSchema schema, string domainNamespace)
        {
            // set up the class and add it in
            var sourceFile = new StringBuilder();
            var constructorStatements = new StringBuilder();
            sourceFile.AppendLine("namespace " + domainNamespace);
            sourceFile.AppendLine("{");
            sourceFile.AppendLine(FourSpaces() + "using System;");
            sourceFile.AppendLine(FourSpaces() + "using System.Collections.Generic;");
            sourceFile.AppendLine();
            sourceFile.AppendLine(FourSpaces() + "public class " + this.convention.ClassNameForTable(map.Table));
            sourceFile.AppendLine(FourSpaces() + "{");
            sourceFile.AppendLine(FourSpaces(2) + "public " + this.convention.ClassNameForTable(map.Table) + "()");
            sourceFile.AppendLine(FourSpaces(2) + "{");
            int constructorInsertionPoint = sourceFile.Length;
            sourceFile.AppendLine(FourSpaces(2) + "}");
            sourceFile.AppendLine();

            // add in the properties
            this.AddColumn(map.PrimaryKey, sourceFile, constructorStatements, schema);
            foreach (var column in map.Columns.Where(c => c.Key != map.PrimaryKey.Name))
            {
                this.AddColumn(column.Value, sourceFile, constructorStatements, schema);
            }


            sourceFile.AppendLine(FourSpaces() + "}");
            sourceFile.AppendLine("}");

            // insert constructor statements
            sourceFile.Insert(constructorInsertionPoint, constructorStatements.ToString());

            result.Add(sourceFile.ToString());
        }

        private void AddColumn(IColumn column, StringBuilder sourceFile, StringBuilder constructorStatements, DatabaseSchema schema)
        {
            if (column.Relationship == RelationshipType.None)
            {
                this.AddProperty(sourceFile, column.Type.ToString(), column.Name);
            }
            else if (column.Relationship == RelationshipType.ManyToOne)
            {
                this.AddProperty(sourceFile, this.convention.ClassNameForTable(schema.Tables.First(t => t.Name == column.Map.Table).FindColumn(column.DbName).ForeignKeyTableName), column.Name);
            }
            else if (column.Relationship == RelationshipType.OneToMany)
            {
                var typeName = column.Type.GetGenericArguments()[0].Name;
                this.AddProperty(sourceFile, "IList<" + typeName + ">", column.Name);
                constructorStatements.AppendLine(FourSpaces(3) + "this." + column.Name + " = new List<" + typeName + ">();");
            }
            else if (column.Relationship == RelationshipType.ManyToMany)
            {
                // this should be fairly simple - find manytomany table and find other side of relationship
                throw new NotImplementedException();
            }
        }

        private void AddProperty(StringBuilder sourceFile, string type, string name)
        {
            sourceFile.AppendLine(FourSpaces(2) + "public " + type + " " + name + " { get; set; }");
            sourceFile.AppendLine();
        }

        private static string FourSpaces(int multiple = 1)
        {
            string fourSpaces = "    ";
            var result = new StringBuilder();
            for (int i = 0; i < multiple; ++i)
            {
                result.Append(fourSpaces);
            }

            return result.ToString();
        }
    }
}
