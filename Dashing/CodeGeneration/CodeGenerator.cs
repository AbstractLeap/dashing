namespace Dashing.CodeGeneration {
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dapper;

    using Dashing.Configuration;

    public class CodeGenerator : ICodeGenerator {
        private readonly CodeGeneratorConfig generatorConfig;

        private readonly IProxyGenerator proxyGenerator;

        private readonly IDapperWrapperGenerator dapperWrapperGenerator;

        public long ElapsedMilliseconds { get; private set; }

        public CodeGeneratorConfig Configuration {
            get {
                return this.generatorConfig;
            }
        }

        public CodeGenerator(CodeGeneratorConfig codeGeneratorConfiguration, IProxyGenerator proxyGenerator, IDapperWrapperGenerator dapperWrapperGenerator) {
            this.generatorConfig = codeGeneratorConfiguration;
            this.proxyGenerator = proxyGenerator;
            this.dapperWrapperGenerator = dapperWrapperGenerator;
        }

        public IGeneratedCodeManager Generate(IConfiguration configuration) {
            // Look for an assembly that was already loaded 
            foreach (var generatedCodeAssembly in
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetReferencedAssemblies().Where(a => a.Name == this.generatorConfig.Namespace))) {
                return new GeneratedCodeManager(this.generatorConfig, Assembly.Load(generatedCodeAssembly), configuration);
            }

            var timer = new Stopwatch();
            timer.Start();
            var maps = configuration.Maps.ToArray();
            var mapDict = configuration.Maps.ToDictionary(m => m.Type, m => m); // really don't like this pattern

            // generate proxies
            var proxyGeneratorResult = this.proxyGenerator.GenerateProxies(this.generatorConfig, mapDict);

            // generate query layer
            var dapperWrapper = this.dapperWrapperGenerator.GenerateDapperWrapper(this.generatorConfig, maps, configuration.GetMap);

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
                OutputAssembly = this.generatorConfig.OutputAssembly ? this.generatorConfig.AssemblyPath : null, 
                CompilerOptions = this.generatorConfig.CompileInDebug ? string.Empty : "/optimize"
            };

            // ok, so far so abstract
            var provider = CodeDomProvider.CreateProvider("CSharp");

            using (var sw = new StringWriter()) {
                // write the code into the string
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions { BracingStyle = "C" });

                // do some hinky string replace because the DOM doesnt have the right method
                var source = sw.ToString()
                    .Replace("DelegateQuery(", "DelegateQuery<T>(")
                    .Replace("DelegateQueryAsync(", "DelegateQueryAsync<T>(")
                    .Replace("Task<IEnumerable<T>> QueryAsync", "async Task<IEnumerable<T>> QueryAsync")
                    .Replace("return methAsync", "return await methAsync")
                    .Replace("return SqlMapper.QueryAsync", "return await SqlMapper.QueryAsync")
                    .Replace("static Task<", "async static Task<");

                var results = provider.CompileAssemblyFromSource(compilerParameters, source);

                timer.Stop();
                this.ElapsedMilliseconds = timer.ElapsedMilliseconds;

                // write the source
                if (this.generatorConfig.OutputSourceCode) {
                    var annotatedsource = string.Format("// Generated on {0} in {1}ms {2}", DateTime.Now, this.ElapsedMilliseconds, Environment.NewLine) + source;
                    File.WriteAllText(this.generatorConfig.SourceCodePath, annotatedsource);
                }

                if (results.Errors.HasErrors) {
                    var firstError = results.Errors[0];
                    var context = string.Join(Environment.NewLine, source.Split('\n').Select((l, i) => string.Format("{0,-4}: {1}", i, l).Trim()).Skip(firstError.Line - 5).Take(10));
                    throw new Exception(string.Format("Error while compiling generated code: {0} on line {1}\n\n{2}", firstError.ErrorText, firstError.Line, context));
                }

                // return the wrapper
                return new GeneratedCodeManager(this.generatorConfig, results.CompiledAssembly, configuration);
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
            yield return "System.Threading.Tasks";
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
    }
}