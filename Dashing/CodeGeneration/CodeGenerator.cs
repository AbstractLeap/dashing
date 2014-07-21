namespace Dashing.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.DML;
    using Dashing.Extensions;

    // TODO: Use Refly instead of CodeDom - http://www.codeproject.com/Articles/6283/Refly-makes-the-CodeDom-er-life-easier
    public class CodeGenerator : ICodeGenerator {
        private readonly CodeGeneratorConfig generatorConfig;

        private readonly IProxyGenerator proxyGenerator;

        public long ElapsedMilliseconds { get; private set; }

        public CodeGeneratorConfig Configuration { 
            get { 
                return this.generatorConfig; 
            } 
        }

        public CodeGenerator(CodeGeneratorConfig codeGeneratorConfiguration, IProxyGenerator proxyGenerator) {
            this.generatorConfig = codeGeneratorConfiguration;
            this.proxyGenerator = proxyGenerator;
        }

        public IGeneratedCodeManager Generate(IConfiguration configuration) {
            // Look for an assembly that was already loaded 
            foreach (var generatedCodeAssembly in
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetReferencedAssemblies().Where(a => a.Name == this.generatorConfig.Namespace))) {
                return new GeneratedCodeManager(this.generatorConfig, Assembly.Load(generatedCodeAssembly));
            }

            var timer = new Stopwatch();
            timer.Start();
            var maps = configuration.Maps.ToArray();
            var mapDict = configuration.Maps.ToDictionary(m => m.Type, m => m); // really don't like this pattern

            // generate proxies
            var proxyGeneratorResult = this.proxyGenerator.GenerateProxies(this.generatorConfig, mapDict);

            // generate query layer
            var dapperWrapper = this.GenerateDapperWrapper(maps, configuration.GetMap);

            // construct the namespace
            var namespaceImports = proxyGeneratorResult.NamespaceImports.Union(GetStandardNamespaceImports());
            var codeNamespace = new CodeNamespace(this.generatorConfig.Namespace);
            codeNamespace.Imports.AddRange(namespaceImports.ToArray());
            codeNamespace.Types.AddRange(proxyGeneratorResult.ProxyTypes.ToArray());
            codeNamespace.Types.Add(dapperWrapper);

            // construct the compile unit
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(codeNamespace);

            // construct the compiler parameters
            var referencedAssemblyLocations = proxyGeneratorResult.ReferencedAssemblyLocations.Union(GetStandardCodeReferences());
            
            var compilerParameters = new CompilerParameters(referencedAssemblyLocations.ToArray()) {
                                                                                                       GenerateInMemory = true,
                                                                                                       IncludeDebugInformation = this.generatorConfig.CompileInDebug,
                                                                                                       OutputAssembly =
                                                                                                           this.generatorConfig.OutputAssembly
                                                                                                               ? this.generatorConfig.AssemblyPath
                                                                                                               : null,
                                                                                                       CompilerOptions =
                                                                                                           this.generatorConfig.CompileInDebug ? string.Empty : "/optimize"
                                                                                                   };

            // ok, so far so abstract
            var provider = CodeDomProvider.CreateProvider("CSharp");

            using (var sw = new StringWriter()) {
                // write the code into the string
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions { BracingStyle = "C" });

                // do some hinky string replace because the DOM doesnt have the right method
                var source = sw.ToString().Replace("DelegateQuery(", "DelegateQuery<T>(");

                var results = provider.CompileAssemblyFromSource(compilerParameters, source);

                timer.Stop();
                this.ElapsedMilliseconds = timer.ElapsedMilliseconds;

                // write the source
                if (this.generatorConfig.OutputSourceCode) {
                    var annotatedsource = string.Format("// Generated on {0} in {1}ms {2}", DateTime.Now, this.ElapsedMilliseconds, Environment.NewLine) + source;
                    File.WriteAllText(this.generatorConfig.SourceCodePath, annotatedsource);
                }

                if (results.Errors.HasErrors) {
                    throw new Exception(results.Errors[0].ErrorText);
                }

                // return the wrapper
                return new GeneratedCodeManager(this.generatorConfig, results.CompiledAssembly);
            }
        }

        private static IEnumerable<CodeNamespaceImport> GetStandardNamespaceImports() {
            return GetStandardNamespaces().Select(ns => new CodeNamespaceImport(ns));
        }

        private static IEnumerable<string> GetStandardNamespaces() {
            yield return "System";
            yield return "System.Collections.Generic";
            yield return "System.Data";
            yield return "System.Diagnostics";
            yield return "Dapper";
            yield return "Dashing.Engine";
            yield return "Dashing.Engine.DML";
        }

        private static IEnumerable<string> GetStandardCodeReferences() {
            yield return "System.dll";
            yield return "System.Core.dll";
            yield return "System.Data.dll";

            yield return Assembly.GetExecutingAssembly().Location; // Dashing.dll
            yield return Assembly.GetAssembly(typeof(SqlMapper)).Location; // Dapper.dll
        }

        private CodeTypeDeclaration GenerateDapperWrapper(IEnumerable<IMap> maps, Func<Type, IMap> getMap) {
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
            var del = new CodeTypeDelegate("DelegateQuery");
            del.TypeParameters.Add("T");
            del.ReturnType = new CodeTypeReference("IEnumerable<T>");
            del.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<T>", "query"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "connection"));
            del.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));
            dapperWrapperClass.Members.Add(del);

            // generate the type delegates dictionary
            var delegatesField = new CodeMemberField(
                typeof(IDictionary<,>).MakeGenericType(typeof(Type), typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate))),
                "TypeDelegates");
            delegatesField.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            dapperWrapperClass.Members.Add(delegatesField);
            staticConstructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), "TypeDelegates"),
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(Dictionary<,>).MakeGenericType(typeof(Type), typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate)))))));

            // generate the query method
            var query = new CodeMemberMethod();
            query.Name = "Query";
            query.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            query.TypeParameters.Add("T");
            query.ReturnType = new CodeTypeReference("IEnumerable<T>");
            dapperWrapperClass.Members.Add(query);
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<T>", "query"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "connection"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));

            //// var meth = (DelegateQuery<T>)TypeDelegates[typeof(T)][result.FetchTree.FetchSignature];
            //// return meth(result, query, conn);
            query.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "var",
                    "meth",
                    new CodeCastExpression(
                        "DelegateQuery<T>",
                        new CodeIndexerExpression(
                            new CodeIndexerExpression(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), "TypeDelegates"),
                                new CodeTypeOfExpression("T")),
                            new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "FetchTree"), "FetchSignature")))));
            query.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeDelegateInvokeExpression(
                        new CodeVariableReferenceExpression("meth"),
                        new CodeVariableReferenceExpression("result"),
                        new CodeVariableReferenceExpression("query"),
                        new CodeVariableReferenceExpression("connection"),
                        new CodeVariableReferenceExpression("transaction"))));

            // now foreach type we wish to find all possible fetch trees (up to a certain depth) and generate mappers and query functions
            foreach (var map in maps) {
                // TODO: Support fetching collections
                var rootNode = new FetchNode();
                var signatures = this.TraverseAndGenerateMappersAndQueries(
                    dapperWrapperClass,
                    rootNode,
                    rootNode,
                    map.Type,
                    map.Type,
                    getMap,
                    0,
                    this.generatorConfig.MapperGenerationMaxRecursion,
                    this.generatorConfig);

                // now add in the dictionary statement
                var delegateField = new CodeMemberField(typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate)), map.Type.Name + "Delegates");
                delegateField.Attributes = MemberAttributes.Static;
                dapperWrapperClass.Members.Add(delegateField);

                staticConstructor.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), map.Type.Name + "Delegates"),
                        new CodeObjectCreateExpression(new CodeTypeReference(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(Delegate))))));

                foreach (var signature in signatures) {
                    staticConstructor.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), map.Type.Name + "Delegates"),
                            "Add",
                            new CodeSnippetExpression("\"" + signature.Item1 + "\""),
                            new CodeObjectCreateExpression("DelegateQuery<" + map.Type.Name + ">", new CodeSnippetExpression(signature.Item2))));
                }

                staticConstructor.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), delegatesField.Name),
                        "Add",
                        new CodeSnippetExpression("typeof(" + map.Type.Name + ")"),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("DapperWrapper"), map.Type.Name + "Delegates")));
            }

            return dapperWrapperClass;
        }

        private IEnumerable<Tuple<string, string>> TraverseAndGenerateMappersAndQueries(
            CodeTypeDeclaration dapperWrapperClass,
            FetchNode rootNode,
            FetchNode currentPath,
            Type rootType,
            Type currentType,
            Func<Type, IMap> getMap,
            int recursionLevel,
            int maxRecursion,
            CodeGeneratorConfig codeConfig,
            string signaturePrefix = "",
            string signatureSuffix = "") {
            var map = getMap(currentType);
            var manyToOneColumns = map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne);
            var signatures = new List<Tuple<string, string>>();
            foreach (var subset in manyToOneColumns.Subsets().Where(s => s.Any())) {
                // we need to generate a mapping function and a query function
                var orderedSubset = subset.OrderBy(c => c.Value.FetchId);

                // first generate mappers at this level then go down
                string thisSignature = string.Join("SE", orderedSubset.Select(c => c.Value.FetchId)) + "SE";
                foreach (var column in orderedSubset) {
                    currentPath.Children.Add(column.Key, new FetchNode { Column = column.Value });
                }

                var dictionaryInitialiser = this.GenerateMappersAndQueries(dapperWrapperClass, rootNode, rootType, signaturePrefix + thisSignature + signatureSuffix, codeConfig);
                signatures.AddRange(dictionaryInitialiser);

                // we have to limit recursion level otherwise possible to get stuck in infinite loop
                if (recursionLevel < maxRecursion) {
                    int currentSplitPoint = 0;
                    foreach (var column in orderedSubset) {
                        var childSignaturePrefix = thisSignature.Substring(0, currentSplitPoint + column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 1);
                        var childSignatureSuffix = thisSignature.Substring(currentSplitPoint + column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 1);
                        var childSignatures = this.TraverseAndGenerateMappersAndQueries(
                            dapperWrapperClass,
                            rootNode,
                            currentPath.Children.First(c => c.Key == column.Key).Value,
                            rootType,
                            column.Value.Type,
                            getMap,
                            recursionLevel + 1,
                            maxRecursion,
                            codeConfig,
                            signaturePrefix + childSignaturePrefix,
                            childSignatureSuffix + signatureSuffix);
                        currentSplitPoint += column.Value.FetchId.ToString(CultureInfo.InvariantCulture).Length + 2;
                        signatures.AddRange(childSignatures);
                    }
                }

                currentPath.Children.Clear();
            }

            return signatures;
        }

        private IEnumerable<Tuple<string, string>> GenerateMappersAndQueries(
            CodeTypeDeclaration dapperWrapperClass,
            FetchNode rootNode,
            Type rootType,
            string signature,
            CodeGeneratorConfig codeConfig) {
            // generate the fk and tracked mappers
            var foreignKeyMapper = this.GenerateMapper(dapperWrapperClass, rootNode, rootType, signature, codeConfig.ForeignKeyAccessClassSuffix);
            var trackingMapper = this.GenerateMapper(dapperWrapperClass, rootNode, rootType, signature, codeConfig.TrackedClassSuffix);

            // Generate the query method
            var query = this.GenerateQueryMethod(dapperWrapperClass, rootType, signature, trackingMapper, foreignKeyMapper, codeConfig);

            return new List<Tuple<string, string>> { new Tuple<string, string>(signature, query.Name) };
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private CodeMemberMethod GenerateQueryMethod(
            CodeTypeDeclaration dapperWrapperClass,
            Type rootType,
            string signature,
            CodeMemberMethod trackingMapper,
            CodeMemberMethod foreignKeyMapper,
            CodeGeneratorConfig codeConfig) {
            // TODO: Add in mapped on
            var query = new CodeMemberMethod();
            query.Name = rootType.Name + "_" + signature;
            query.ReturnType = new CodeTypeReference("IEnumerable<" + rootType.Name + ">");
            query.Attributes = MemberAttributes.Static;
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectWriterResult", "result"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("SelectQuery<" + rootType.Name + ">", "query"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "conn"));
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbTransaction", "transaction"));

#if DEBUG
            query.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("Debug"), 
                    "Write", 
                    new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "FetchTree"), "SplitOn")));
#endif

            var returnStatement =
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("SqlMapper"), "Query"),
                        new CodeExpression[] {
                                                 new CodeVariableReferenceExpression("conn"), 
                                                 new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Sql"),
                                                 new CodeVariableReferenceExpression("mapper"), 
                                                 new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Parameters"),
                                                 new CodeVariableReferenceExpression("transaction"), 
                                                 new CodePrimitiveExpression(true),
                                                 new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "FetchTree"), "SplitOn")
                                             }));

            var trackingDeclaration =
                new CodeVariableDeclarationStatement(
                    "Func`" + (trackingMapper.Parameters.Count + 1) + "["
                    + trackingMapper.Parameters.Cast<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType).First() + ","
                    + string.Join(
                        ", ",
                        trackingMapper.Parameters.Cast<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType + codeConfig.ForeignKeyAccessClassSuffix).Skip(1)) + ", "
                    + trackingMapper.Parameters.Cast<CodeParameterDeclarationExpression>().First().Type.BaseType + "]",
                    "mapper",
                    new CodeMethodReferenceExpression(null, trackingMapper.Name));

            var foreignKeyDeclaration =
                new CodeVariableDeclarationStatement(
                    "Func`" + (foreignKeyMapper.Parameters.Count + 1) + "["
                    + foreignKeyMapper.Parameters.Cast<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType).First() + ","
                    + string.Join(
                        ", ",
                        foreignKeyMapper.Parameters.Cast<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType + codeConfig.ForeignKeyAccessClassSuffix).Skip(1)) + ", "
                    + foreignKeyMapper.Parameters.Cast<CodeParameterDeclarationExpression>().First().Type.BaseType + "]",
                    "mapper",
                    new CodeMethodReferenceExpression(null, foreignKeyMapper.Name));

            query.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("query"), "IsTracked"),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(true)),
                    new CodeStatement[] { trackingDeclaration, returnStatement },
                    new CodeStatement[] { foreignKeyDeclaration, returnStatement }));

            dapperWrapperClass.Members.Add(query);
            return query;
        }

        private CodeMemberMethod GenerateMapper(CodeTypeDeclaration dapperWrapperClass, FetchNode rootNode, Type rootType, string signature, string suffix) {
            var mapper = new CodeMemberMethod();
            mapper.Name = rootType.Name + "_" + signature + suffix;
            mapper.ReturnType = new CodeTypeReference(rootType.Name + suffix);
            mapper.Attributes = MemberAttributes.Static;

            const string RootName = "root";
            mapper.Parameters.Add(new CodeParameterDeclarationExpression(rootType.Name + suffix, RootName));
            foreach (var node in rootNode.Children) {
                this.AddParameterAndAssignment(mapper, RootName, node);
            }

            // add a return statement
            mapper.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(RootName)));

            dapperWrapperClass.Members.Add(mapper);

            return mapper;
        }

        private void AddParameterAndAssignment(CodeMemberMethod mapper, string previousName, KeyValuePair<string, FetchNode> node) {
            var nodeTypeName = node.Value.Column.Type.Name;
            var nodeTypeLowerName = nodeTypeName.ToLower() + Guid.NewGuid().ToString("N").ToLower();
            mapper.Parameters.Add(new CodeParameterDeclarationExpression(nodeTypeName, nodeTypeLowerName));
            mapper.Statements.Add(
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(previousName), node.Key),
                    new CodeVariableReferenceExpression(nodeTypeLowerName)));
            foreach (var child in node.Value.Children) {
                this.AddParameterAndAssignment(mapper, nodeTypeLowerName, child);
            }
        }
    }
}