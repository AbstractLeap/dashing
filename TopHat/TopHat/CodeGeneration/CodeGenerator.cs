namespace TopHat.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Extensions;

    // TODO: Use Refly instead of CodeDom - http://www.codeproject.com/Articles/6283/Refly-makes-the-CodeDom-er-life-easier
    internal class CodeGenerator : ICodeGenerator {
        private readonly CodeGeneratorConfig generatorConfig;

        public long ElapsedMilliseconds { get; private set; }

        public CodeGenerator(CodeGeneratorConfig codeGeneratorConfiguration) {
            this.generatorConfig = codeGeneratorConfiguration;
        }

        // TODO: sense if we can just reuse last times ?
        public IGeneratedCodeManager Generate(IConfiguration configuration) {
            var timer = new Stopwatch();
            timer.Start();
            var maps = configuration.Maps.ToArray();

            // generate proxies
            var parallelMaps = maps.AsParallel();
            var extendedTypes = parallelMaps.Select(m => this.CreateFkClass(m, configuration.GetMap));
            var trackingTypes = parallelMaps.Select(this.CreateTrackingClass);

            // generate query layer
            var dapperWrapper = this.GenerateDapperWrapper(maps, configuration.GetMap);

            // extract metadata from map types
            var mappedTypes = maps.Select(m => m.Type).ToArray();
            var mapNamespaces = mappedTypes.Select(type => type.Namespace).Distinct().Union(new[] { "System", "System.Collections.Generic", "System.Data", "System.Diagnostics", "Dapper", "TopHat.Engine" });

            // construct the reference space
            var codeReferences = mappedTypes.Select(type => type.Assembly)
                .Distinct()
                .Select(a => a.GetName().Name + ".dll")
                .Union(new[] { "System.dll", "System.Core.dll", "System.Data.dll", "TopHat.dll", "Dapper.dll" });

            // construct the namespace
            var codeNamespace = new CodeNamespace(this.generatorConfig.Namespace);
            codeNamespace.Imports.AddRange(mapNamespaces.Select(ns => new CodeNamespaceImport(ns)).ToArray());
            codeNamespace.Types.AddRange(extendedTypes.ToArray());
            codeNamespace.Types.AddRange(trackingTypes.ToArray());
            codeNamespace.Types.Add(dapperWrapper);

            // construct the compile unit
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(codeNamespace);

            // ok, so far so abstract
            var provider = CodeDomProvider.CreateProvider("CSharp");

            using (var sw = new StringWriter()) {
                // write the code into the string
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions { BracingStyle = "C" });

                // do some hinky string replace because the DOM doesnt have the right method
                var source = sw.ToString().Replace("DelegateQuery(", "DelegateQuery<T>(");

                // generate the assembly
                var results = provider.CompileAssemblyFromSource(new CompilerParameters(codeReferences.ToArray(), this.generatorConfig.Namespace + ".dll", true), source);

                timer.Stop();
                this.ElapsedMilliseconds = timer.ElapsedMilliseconds;
                
                // write the source
                if (this.generatorConfig.GenerateSource) {
                    var annotatedsource = string.Format("// Generated on {0} in {1}ms \n", DateTime.Now, this.ElapsedMilliseconds) + source;
                    File.WriteAllText(this.generatorConfig.Namespace + ".cs", annotatedsource);
                }

                // return the wrapper
                return new GeneratedCodeManager(this.generatorConfig, results.CompiledAssembly);
            }
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
            del.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "conn"));
            dapperWrapperClass.Members.Add(del);

            // generate the type delegates dictionary
            var delegatesField = new CodeMemberField(
                typeof(IDictionary<,>).MakeGenericType(typeof(Type), typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(Delegate))),
                "TypeDelegates");
            delegatesField.Attributes = MemberAttributes.Static;
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
            query.Parameters.Add(new CodeParameterDeclarationExpression("IDbConnection", "conn"));

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
                        new CodeVariableReferenceExpression("conn"))));

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

                var dictionaryInitialiser = this.GenerateMappersAndQueries(
                    dapperWrapperClass,
                    rootNode,
                    rootType,
                    signaturePrefix + thisSignature + signatureSuffix,
                    codeConfig);
                signatures.AddRange(dictionaryInitialiser);

                // we have to limit recursion level otherwise possible to get stuck in infinite loop
                if (recursionLevel < maxRecursion) {
                    int currentSplitPoint = 0;
                    foreach (var column in orderedSubset) {
                        var childSignaturePrefix = thisSignature.Substring(0, currentSplitPoint + column.Value.FetchId.ToString().Length + 1);
                        var childSignatureSuffix = thisSignature.Substring(currentSplitPoint + column.Value.FetchId.ToString().Length + 1);
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
                        currentSplitPoint += column.Value.FetchId.ToString().Length + 2;
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
                                                 new CodeVariableReferenceExpression("conn"), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Sql"),
                                                 new CodeVariableReferenceExpression("mapper"), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("result"), "Parameters"),
                                                 new CodePrimitiveExpression(null), new CodePrimitiveExpression(true),
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

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private CodeTypeDeclaration CreateTrackingClass(IMap map) {
            var trackingClass = new CodeTypeDeclaration(map.Type.Name + this.generatorConfig.TrackedClassSuffix);
            trackingClass.IsClass = true;
            trackingClass.TypeAttributes = TypeAttributes.Public;
            trackingClass.BaseTypes.Add(map.Type.Name + this.generatorConfig.ForeignKeyAccessClassSuffix);
            trackingClass.BaseTypes.Add(typeof(ITrackedEntity));

            // add in change tracking properties
            this.GenerateGetSetProperty(trackingClass, "IsTracking", typeof(bool), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(trackingClass, "DirtyProperties", typeof(ISet<>).MakeGenericType(typeof(string)), MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(
                trackingClass,
                "OldValues",
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)),
                MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(
                trackingClass,
                "NewValues",
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)),
                MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(
                trackingClass,
                "AddedEntities",
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))),
                MemberAttributes.Public | MemberAttributes.Final);
            this.GenerateGetSetProperty(
                trackingClass,
                "DeletedEntities",
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))),
                MemberAttributes.Public | MemberAttributes.Final);

            // add in a constructor to initialise collections
            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Statements.Add(
                new CodeAssignStatement(CodeHelpers.ThisField("DirtyProperties"), new CodeObjectCreateExpression(typeof(HashSet<>).MakeGenericType(typeof(string)))));
            constructor.Statements.Add(
                new CodeAssignStatement(CodeHelpers.ThisField("OldValues"), new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)))));
            constructor.Statements.Add(
                new CodeAssignStatement(CodeHelpers.ThisField("NewValues"), new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("AddedEntities"),
                    new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))))));
            constructor.Statements.Add(
                new CodeAssignStatement(
                    CodeHelpers.ThisField("DeletedEntities"),
                    new CodeObjectCreateExpression(typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))))));

            // these constructor statements override the collection properties to use observable collections
            foreach (var collectionColumn in map.Columns.Where(c => c.Value.Type.IsCollection())) {
                constructor.Statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(null)),
                        new CodeStatement[] {
                                                new CodeAssignStatement(
                                                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                                                    new CodeObjectCreateExpression(
                                                    "TopHat.CodeGeneration.TrackingCollection<" + trackingClass.Name + "," + collectionColumn.Value.Type.GenericTypeArguments.First() + ">",
                                                    new CodeThisReferenceExpression(),
                                                    new CodePrimitiveExpression(collectionColumn.Key)))
                                            },
                        new CodeStatement[] {
                                                new CodeAssignStatement(
                                                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key),
                                                    new CodeObjectCreateExpression(
                                                    "TopHat.CodeGeneration.TrackingCollection<" + trackingClass.Name + "," + collectionColumn.Value.Type.GenericTypeArguments.First() + ">",
                                                    new CodeThisReferenceExpression(),
                                                    new CodePrimitiveExpression(collectionColumn.Key),
                                                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), collectionColumn.Key)))
                                            }));
            }

            // override value type properties to perform dirty checking
            foreach (var valueTypeColumn in map.Columns.Where(c => !c.Value.Type.IsCollection() && !c.Value.IsIgnored)) {
                var prop = this.GenerateGetSetProperty(trackingClass, valueTypeColumn.Key, valueTypeColumn.Value.Type, MemberAttributes.Public | MemberAttributes.Override, true);

                // override the setter
                // if isTracking && !this.DirtyProperties.ContainsKey(prop) add to dirty props and add oldvalue
                bool propertyCanBeNull = valueTypeColumn.Value.Type.IsNullable() || !valueTypeColumn.Value.Type.IsValueType;
                var changeCheck = new CodeBinaryOperatorExpression();
                if (!propertyCanBeNull) {
                    // can't be null so just check values
                    changeCheck.Left = new CodeMethodInvokeExpression(CodeHelpers.BaseProperty(valueTypeColumn.Key), "Equals", new CodePropertySetValueReferenceExpression());
                    changeCheck.Operator = CodeBinaryOperatorType.IdentityEquality;
                    changeCheck.Right = new CodePrimitiveExpression(false);
                }
                else {
                    // can be null, need to be careful of null reference exceptions
                    changeCheck.Left = new CodeBinaryOperatorExpression(
                        CodeHelpers.BasePropertyIsNull(valueTypeColumn.Key),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodePropertySetValueReferenceExpression(),
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)));
                    changeCheck.Operator = CodeBinaryOperatorType.BooleanOr;
                    changeCheck.Right = new CodeBinaryOperatorExpression(
                        CodeHelpers.BasePropertyIsNotNull(valueTypeColumn.Key),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(CodeHelpers.BaseProperty(valueTypeColumn.Key), "Equals", new CodePropertySetValueReferenceExpression()),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(false)));
                }

                prop.SetStatements.Insert(
                    0,
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            CodeHelpers.ThisPropertyIsTrue("IsTracking"),
                            CodeBinaryOperatorType.BooleanAnd,
                            new CodeBinaryOperatorExpression(
                                new CodeBinaryOperatorExpression(
                                    new CodeMethodInvokeExpression(CodeHelpers.ThisProperty("DirtyProperties"), "Contains", new CodePrimitiveExpression(prop.Name)),
                                    CodeBinaryOperatorType.IdentityEquality,
                                    new CodePrimitiveExpression(false)),
                                CodeBinaryOperatorType.BooleanAnd,
                                changeCheck)),
                        new CodeStatement[] {
                                                new CodeExpressionStatement(new CodeMethodInvokeExpression(CodeHelpers.ThisProperty("DirtyProperties"), "Add", new CodePrimitiveExpression(prop.Name))),
                                                new CodeAssignStatement(
                                                    new CodeIndexerExpression(CodeHelpers.ThisProperty("OldValues"), new CodePrimitiveExpression(prop.Name)),
                                                    new CodePropertySetValueReferenceExpression())
                                            },
                        new CodeStatement[] {
                                                new CodeConditionStatement(
                                                    CodeHelpers.ThisPropertyIsTrue("IsTracking"),
                                                    new CodeAssignStatement(
                                                    new CodeIndexerExpression(CodeHelpers.ThisProperty("NewValues"), new CodePrimitiveExpression(prop.Name)),
                                                    new CodePropertySetValueReferenceExpression()))
                                            }));
            }

            trackingClass.Members.Add(constructor);

            return trackingClass;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "This is hard to read the StyleCop way")]
        private CodeTypeDeclaration CreateFkClass(IMap map, Func<Type, IMap> getMap) {
            // generate the foreign key access class based on the original class
            var foreignKeyClass = new CodeTypeDeclaration(map.Type.Name + this.generatorConfig.ForeignKeyAccessClassSuffix);
            foreignKeyClass.IsClass = true;
            foreignKeyClass.TypeAttributes = TypeAttributes.Public;
            foreignKeyClass.BaseTypes.Add(map.Type);

            foreach (var column in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne)) {
                // create a backing property for storing the FK
                var backingType = column.Value.DbType.GetCLRType();
                if (backingType.IsValueType) {
                    backingType = typeof(Nullable<>).MakeGenericType(backingType);
                }

                var foreignKeyBackingProperty = this.GenerateGetSetProperty(foreignKeyClass, column.Value.DbName, backingType, MemberAttributes.Public | MemberAttributes.Final);

                // create a backing field for storing the related entity
                var backingField = new CodeMemberField(column.Value.Type, column.Value.Name + this.generatorConfig.ForeignKeyAccessEntityFieldSuffix);
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
                                                new CodeMethodReturnStatement(CodeHelpers.ThisField(backingField.Name))
                                            },
                        new CodeStatement[] {
                                                // false, return new object with foreign key set
                                                new CodeVariableDeclarationStatement(column.Value.Type, "val", new CodeObjectCreateExpression(column.Value.Type)),
                                                new CodeAssignStatement(
                                                    new CodeFieldReferenceExpression(
                                                    new CodeVariableReferenceExpression("val"),
                                                    getMap(column.Value.Type).PrimaryKey.Name),
                                                    new CodePropertyReferenceExpression(CodeHelpers.ThisProperty(foreignKeyBackingProperty.Name), "Value")),
                                                new CodeAssignStatement(CodeHelpers.ThisField(backingField.Name), new CodeVariableReferenceExpression("val")),
                                                new CodeMethodReturnStatement(new CodeVariableReferenceExpression("val"))
                                            }));
                property.SetStatements.Add(new CodeAssignStatement(CodeHelpers.ThisField(backingField.Name), new CodePropertySetValueReferenceExpression()));
                foreignKeyClass.Members.Add(property);
            }

            return foreignKeyClass;
        }

        private CodeMemberProperty GenerateGetSetProperty(CodeTypeDeclaration owningClass, string name, Type type, MemberAttributes attributes, bool useBaseProperty = false) {
            // generate the property
            var prop = new CodeMemberProperty();
            prop.Name = name;
            prop.Type = new CodeTypeReference(type);
            prop.Attributes = attributes;

            if (useBaseProperty) {
                prop.GetStatements.Add(new CodeMethodReturnStatement(CodeHelpers.BaseProperty(name)));
                prop.SetStatements.Add(new CodeAssignStatement(CodeHelpers.BaseProperty(name), new CodePropertySetValueReferenceExpression()));
            }
            else {
                // generate the backing field for this property
                var backingField = new CodeMemberField();
                backingField.Name = "backing" + name;
                backingField.Type = new CodeTypeReference(type);
                owningClass.Members.Add(backingField);

                prop.GetStatements.Add(new CodeMethodReturnStatement(CodeHelpers.ThisField(backingField.Name)));
                prop.SetStatements.Add(new CodeAssignStatement(CodeHelpers.ThisField(backingField.Name), new CodePropertySetValueReferenceExpression()));
            }

            owningClass.Members.Add(prop);

            return prop;
        }
    }
}