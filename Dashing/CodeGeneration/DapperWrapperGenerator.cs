namespace Dashing.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Extensions;

    /// <summary>
    /// Generate the wrapper around several Dapper methods
    /// </summary>
    public class DapperWrapperGenerator : IDapperWrapperGenerator {
        /// <summary>
        /// Generate the declarations for the dapper wrapper class
        /// </summary>
        public CodeTypeDeclaration GenerateDapperWrapper(CodeGeneratorConfig generatorConfig, IEnumerable<IMap> maps, Func<Type, IMap> getMap) {
            // can't create a static class using CodeDom so create a Public Sealed class with private constructor
            var dapperWrapperClass = new CodeTypeDeclaration("DapperWrapper");
            dapperWrapperClass.IsClass = true;
            dapperWrapperClass.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
            dapperWrapperClass.Attributes = MemberAttributes.Static;

            var privateConstructor = new CodeConstructor();
            privateConstructor.Attributes = MemberAttributes.Private;
            dapperWrapperClass.Members.Add(privateConstructor);

            var staticConstructor = new CodeTypeConstructor();
            dapperWrapperClass.Members.Add(staticConstructor);

            // generate the delegate function
            // public delegate IEnumerable<T> DelegateQuery<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection conn);
            GetCodeTypeDelegate(dapperWrapperClass, "DelegateQuery", "IEnumerable<T>");

            // generate an async delegate function
            GetCodeTypeDelegate(dapperWrapperClass, "DelegateQueryAsync", "Task<IEnumerable<T>>");

            // generate the type delegates dictionary
            var delegatesField = GenerateTypeDelegatesDictionary(dapperWrapperClass, staticConstructor, "TypeDelegates");
            var asyncDelegatesField = GenerateTypeDelegatesDictionary(
                dapperWrapperClass,
                staticConstructor,
                "TypeDelegatesAsync");

            // generate the query method
            var query = GenerateQueryMethod(dapperWrapperClass, "Query", "IEnumerable<T>");
            var asyncQuery = GenerateQueryMethod(
                dapperWrapperClass,
                "QueryAsync",
                "Task<IEnumerable<T>>");

            //// var meth = (DelegateQuery<T>)TypeDelegates[typeof(T)][result.FetchTree.FetchSignature];
            //// return meth(result, query, conn);
            GenerateQueryBody(query, "DelegateQuery<T>", "TypeDelegates", "meth");
            GenerateQueryBody(asyncQuery, "DelegateQueryAsync<T>", "TypeDelegatesAsync", "methAsync");

            // now foreach type we wish to find all possible fetch trees (up to a certain depth) and generate mappers and query functions
            foreach (var map in maps) {
                // TODO: Support fetching collections
                var rootNode = new FetchNode();
                var signatures = this.TraverseAndGenerateMappersAndQueries(dapperWrapperClass, rootNode, rootNode, map.Type, map.Type, getMap, 0, generatorConfig.MapperGenerationMaxRecursion, generatorConfig);

                // now add in the dictionary statement
                GenerateMapDelegatesDictionary(map, dapperWrapperClass, map.Type.Name + "Delegates");
                GenerateMapDelegatesDictionary(
                    map,
                    dapperWrapperClass,
                    map.Type.Name + "DelegatesAsync");

                GenerateMapDictionaryInitialiser(staticConstructor, map.Type.Name + "Delegates");
                GenerateMapDictionaryInitialiser(
                    staticConstructor,
                    map.Type.Name + "DelegatesAsync");

                foreach (var signature in signatures) {
                    if (signature.Item2.EndsWith("Async")) {
                        GenerateMapDictionaryAdder(
                        staticConstructor,
                        map.Type.Name + "DelegatesAsync",
                        "DelegateQueryAsync<" + map.Type.Name + ">",
                        signature);
                    }
                    else {
                        GenerateMapDictionaryAdder(staticConstructor, map.Type.Name + "Delegates", "DelegateQuery<" + map.Type.Name + ">", signature);
                    }
                }

                GenerateTypeDictionaryAdder(staticConstructor, delegatesField, map, map.Type.Name + "Delegates");
                GenerateTypeDictionaryAdder(staticConstructor, asyncDelegatesField, map, map.Type.Name + "DelegatesAsync");
            }

            return dapperWrapperClass;
        }

        private static void GenerateTypeDictionaryAdder(
            CodeTypeConstructor staticConstructor,
            CodeMemberField delegatesField,
            IMap map,
            string mapDictionaryName) {
            staticConstructor.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("DapperWrapper"),
                        delegatesField.Name),
                    "Add",
                    new CodeSnippetExpression("typeof(" + map.Type.Name + ")"),
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("DapperWrapper"),
                        mapDictionaryName)));
        }

        private static void GenerateMapDictionaryAdder(
            CodeTypeConstructor staticConstructor,
           string dictionaryName,
            string delegateType,
            Tuple<string, string> signature) {
            staticConstructor.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("DapperWrapper"),
                        dictionaryName),
                    "Add",
                    new CodeSnippetExpression("\"" + signature.Item1 + "\""),
                    new CodeObjectCreateExpression(
                        delegateType,
                        new CodeSnippetExpression(signature.Item2))));
        }

        private static void GenerateMapDictionaryInitialiser(
            CodeTypeConstructor staticConstructor,
            string name) {
            staticConstructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("DapperWrapper"),
                        name),
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(
                            typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(Delegate))))));
        }

        private static void GenerateMapDelegatesDictionary(IMap map, CodeTypeDeclaration dapperWrapperClass, string name) {
            var delegateField =
                new CodeMemberField(
                    typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate)),
                    name);
            delegateField.Attributes = MemberAttributes.Static;
            dapperWrapperClass.Members.Add(delegateField);
        }

        private static void GenerateQueryBody(CodeMemberMethod query, string delegateName, string typeDelegatesName, string delegateVariableName) {
            query.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "var",
                    delegateVariableName,
                    new CodeCastExpression(
                        delegateName,
                        new CodeIndexerExpression(
                            new CodeIndexerExpression(
                                new CodeFieldReferenceExpression(
                                    new CodeTypeReferenceExpression("DapperWrapper"),
                                    typeDelegatesName),
                                new CodeTypeOfExpression("T")),
                            new CodePropertyReferenceExpression(
                                new CodePropertyReferenceExpression(
                                    new CodeVariableReferenceExpression("result"),
                                    "FetchTree"),
                                "FetchSignature")))));
            query.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeDelegateInvokeExpression(
                        new CodeVariableReferenceExpression(delegateVariableName),
                        new CodeVariableReferenceExpression("result"),
                        new CodeVariableReferenceExpression("query"),
                        new CodeVariableReferenceExpression("connection"),
                        new CodeVariableReferenceExpression("transaction"))));
        }

        private static CodeMemberMethod GenerateQueryMethod(CodeTypeDeclaration dapperWrapperClass, string name, string returnType) {
            var query = new CodeMemberMethod();
            query.Name = name;
            query.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            query.TypeParameters.Add("T");
            query.ReturnType = new CodeTypeReference(returnType);
            dapperWrapperClass.Members.Add(query);
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<T>", "query"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "connection"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));
            return query;
        }

        private static CodeMemberField GenerateTypeDelegatesDictionary(
            CodeTypeDeclaration dapperWrapperClass,
            CodeTypeConstructor staticConstructor,
            string name) {
            var delegatesField =
                new CodeMemberField(
                    typeof(IDictionary<,>).MakeGenericType(
                        typeof(Type),
                        typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate))),
                    name);
            delegatesField.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            dapperWrapperClass.Members.Add(delegatesField);
            staticConstructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression("DapperWrapper"),
                        name),
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(
                            typeof(Dictionary<,>).MakeGenericType(
                                typeof(Type),
                                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate)))))));
            return delegatesField;
        }

        private static void GetCodeTypeDelegate(CodeTypeDeclaration dapperWrapperClass, string delegateName, string returnType) {
            var del = new CodeTypeDelegate(delegateName);
            del.TypeParameters.Add("T");
            del.ReturnType = new CodeTypeReference(returnType);
            del.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<T>", "query"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "connection"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));
            dapperWrapperClass.Members.Add(del);
        }

        private IEnumerable<Tuple<string, string>> TraverseAndGenerateMappersAndQueries(CodeTypeDeclaration dapperWrapperClass, FetchNode rootNode, FetchNode currentPath, Type rootType, Type currentType, Func<Type, IMap> getMap, int recursionLevel, int maxRecursion, CodeGeneratorConfig codeConfig, string signaturePrefix = "", string signatureSuffix = "") {
            var map = getMap(currentType);
            var manyToOneColumns = map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne);
            var signatures = new List<Tuple<string, string>>();
            foreach (var subset in manyToOneColumns.Subsets().Where(s => s.Any())) {
                // we need to generate a mapping function and a query function
                var orderedSubset = subset.OrderBy(c => c.Value.FetchId);

                // first generate mappers at this level then go down
                string thisSignature = string.Join("SE", orderedSubset.Select(c => c.Value.FetchId)) + "SE";
                foreach (var column in orderedSubset) {
                    var fetchNode = new FetchNode {
                        Column = column.Value
                    };
                    currentPath.Children.Add(column.Key, fetchNode);
                }

                var dictionaryInitialiser = this.GenerateMappersAndQueries(dapperWrapperClass, rootNode, rootType, signaturePrefix + thisSignature + signatureSuffix, codeConfig);
                signatures.AddRange(dictionaryInitialiser);

                // we have to limit recursion level otherwise possible to get stuck in infinite loop
                if (recursionLevel < maxRecursion) {
                    int currentSplitPoint = 0;
                    foreach (var column in orderedSubset) {
                        var childSignaturePrefix = thisSignature.Substring(0, currentSplitPoint + column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 1);
                        var childSignatureSuffix = thisSignature.Substring(currentSplitPoint + column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 1);
                        var childSignatures = this.TraverseAndGenerateMappersAndQueries(dapperWrapperClass, rootNode, currentPath.Children.First(c => c.Key == column.Key).Value, rootType, column.Value.Type, getMap, recursionLevel + 1, maxRecursion, codeConfig, signaturePrefix + childSignaturePrefix, childSignatureSuffix + signatureSuffix);
                        currentSplitPoint += column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 2;
                        signatures.AddRange(childSignatures);
                    }
                }

                currentPath.Children.Clear();
            }

            return signatures;
        }

        private IEnumerable<Tuple<string, string>> GenerateMappersAndQueries(CodeTypeDeclaration dapperWrapperClass, FetchNode rootNode, Type rootType, string signature, CodeGeneratorConfig codeConfig) {
            // generate the mapper
            IEnumerable<Type> fetchedTypes;
            var mapper = this.GenerateMapper(dapperWrapperClass, rootNode, rootType, signature, out fetchedTypes);

            // generate the query method
            var query = this.GenerateQueryMethod(dapperWrapperClass, rootNode, rootType, fetchedTypes, signature, mapper, codeConfig, false);
            var asyncQuery = this.GenerateQueryMethod(
                dapperWrapperClass,
                rootNode,
                rootType,
                fetchedTypes,
                signature,
                mapper,
                codeConfig,
                true);

            return new List<Tuple<string, string>> {
                new Tuple<string, string>(signature, query.Name),
                new Tuple<string, string>(signature, asyncQuery.Name)
            };
        }

        // TODO: Add in mapped on <-- what's this?
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private CodeMemberMethod GenerateQueryMethod(CodeTypeDeclaration dapperWrapperClass, FetchNode rootNode, Type rootType, IEnumerable<Type> fetchedTypes, string signature, CodeMemberMethod mapper, CodeGeneratorConfig codeConfig, bool isAsync) {
            var foreignKeyTypes = new List<string>();
            foreignKeyTypes.Add(rootType.Name + codeConfig.ForeignKeyAccessClassSuffix);
            foreignKeyTypes.AddRange(fetchedTypes.Select(t => t.Name + codeConfig.ForeignKeyAccessClassSuffix));

            var trackingTypes = new List<string>();
            trackingTypes.Add(rootType.Name + codeConfig.TrackedClassSuffix);
            trackingTypes.AddRange(fetchedTypes.Select(t => t.Name + codeConfig.TrackedClassSuffix));

            // public static IEnumerable<T> EntityName_FetchSignature(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction)
            var connParameterName = isAsync ? "asyncConn" : "conn";
            var query = new CodeMemberMethod {
                Name = rootType.Name + "_" + signature + (isAsync ? "Async" : string.Empty),
                ReturnType = new CodeTypeReference(isAsync ? "Task<IEnumerable<" + rootType.Name + ">>" : "IEnumerable<" + rootType.Name + ">"),
                Attributes = MemberAttributes.Static
            };
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<" + rootType.Name + ">", "query"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", connParameterName));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));

            // Type[] Types; = new[] { typeof(Entity), ... }
            var typesDeclaration = new CodeVariableDeclarationStatement(typeof(Type[]), "types");
            query.Statements.Add(typesDeclaration);

            // if (query.IsTracked) { types = ... } else { types = ... }
            var isTracking = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("query"), "IsTracked"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true));
            var assignForeignKeyMaps = new CodeAssignStatement(new CodeVariableReferenceExpression("types"), new CodeArrayCreateExpression(typeof(Type[]), foreignKeyTypes.Select(t => new CodeTypeOfExpression(t)).Cast<CodeExpression>().ToArray()));
            var assignTrackingMaps = new CodeAssignStatement(new CodeVariableReferenceExpression("types"), new CodeArrayCreateExpression(typeof(Type[]), trackingTypes.Select(t => new CodeTypeOfExpression(t)).Cast<CodeExpression>().ToArray()));
            var typesAssignment = new CodeConditionStatement(isTracking, new CodeStatement[] { assignTrackingMaps }, new CodeStatement[] { assignForeignKeyMaps });
            query.Statements.Add(typesAssignment);

            // return SqlMapper.Query<TReturn>(conn, result.Sql, types, TheNameOfTheMapperStaticMethod, results.Parameters, transaction, buffered: true, result.FetchTree.SplitOn);
            var returnStatement = new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression("SqlMapper"), isAsync ? "QueryAsync" : "Query", new CodeTypeReference(rootType.Name)),
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression(connParameterName), 
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Sql"), 
                            new CodeVariableReferenceExpression("types"),
                            new CodeMethodReferenceExpression(null, mapper.Name), 
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Parameters"), 
                            new CodeVariableReferenceExpression("transaction"), 
                            new CodePrimitiveExpression(true), 
                            new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "FetchTree"), "SplitOn")
                        }));
            query.Statements.Add(returnStatement);

            // add and return
            dapperWrapperClass.Members.Add(query);
            return query;
        }

        private CodeMemberMethod GenerateMapper(CodeTypeDeclaration dapperWrapperClass, FetchNode rootNode, Type rootType, string signature, out IEnumerable<Type> fetchedTypes) {
            // static RootType RootType_Signature(object[] objects)
            var mapper = new CodeMemberMethod {
                Name = rootType.Name + "_" + signature,
                ReturnType = new CodeTypeReference(rootType.Name),
                Attributes = MemberAttributes.Static
            };
            mapper.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object[]), "objects"));

            // dry, innit
            var objects = new CodeVariableReferenceExpression("objects");

            // var root = (RootType)objects[0];
            mapper.Statements.Add(new CodeVariableDeclarationStatement(rootType, "root", new CodeCastExpression(rootType, new CodeArrayIndexerExpression(objects, new CodePrimitiveExpression(0)))));
            var root = new CodeVariableReferenceExpression("root");

            // root.Post = (Post)objects[i];
            var i = 0;
            var nestedFetchedTypes = new List<Type>();
            foreach (var node in rootNode.Children) {
                IEnumerable<Type> childFetchedTypes;
                mapper.Statements.Add(this.AddAssignment(mapper, root, node, objects, ref i, out childFetchedTypes));
                nestedFetchedTypes.AddRange(childFetchedTypes);
            }

            // add a return statement
            mapper.Statements.Add(new CodeMethodReturnStatement(root));
            dapperWrapperClass.Members.Add(mapper);

            // output fetched types
            fetchedTypes = nestedFetchedTypes;
            return mapper;
        }

        private CodeStatement AddAssignment(CodeMemberMethod mapper, CodeExpression currentObject, KeyValuePair<string, FetchNode> node, CodeVariableReferenceExpression objects, ref int i, out IEnumerable<Type> fetchedTypes) {
            var types = new List<Type> { node.Value.Column.Type };
            var objectRef = new CodePropertyReferenceExpression(currentObject, node.Key);
            var statements = new List<CodeStatement>();

            // add the cast statement
            statements.Add(new CodeAssignStatement(
                objectRef,
                new CodeCastExpression(node.Value.Column.Type, new CodeArrayIndexerExpression(objects, new CodePrimitiveExpression(++i)))));
            var thisI = i;
            foreach (var child in node.Value.Children) {
                IEnumerable<Type> nestedTypes;
                statements.Add(this.AddAssignment(mapper, objectRef, child, objects, ref i, out nestedTypes));
                types.AddRange(nestedTypes);
            }

            // add the if not null wrapper
            var ifStatement =
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeArrayIndexerExpression(objects, new CodePrimitiveExpression(thisI)),
                        CodeBinaryOperatorType.IdentityInequality,
                        new CodePrimitiveExpression(null)),
                    statements.ToArray());

            fetchedTypes = types;
            return ifStatement;
        }
    }
}