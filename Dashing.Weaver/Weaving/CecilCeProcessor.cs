//namespace Dashing.CodeGeneration.Weaving
//{
//    using System.Collections.Generic;
//    using System.IO;

//    using Dashing.CodeGeneration.Weaving.Task;
//    using Dashing.CodeGeneration.Weaving.Weavers;

//    using Mono.Cecil;

//    public class CecilPeProcessor : IPeProcessor
//    {
//        private readonly ILogger logger;

//        public CecilPeProcessor(ILogger logger)
//        {
//            this.logger = logger;
//        }

//        public void Process(string peFilePath, IDictionary<string, IEnumerable<ColumnDefinition>> typesToProcess, IEnumerable<IWeaver> weavers)
//        {
//            // load the assembly
//            var hasSymbols = File.Exists(Path.ChangeExtension(peFilePath, "pdb"));
//            var assemblyResolver = new DefaultAssemblyResolver();
//            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(peFilePath));
//            var assembly = AssemblyDefinition.ReadAssembly(
//                peFilePath,
//                new ReaderParameters { ReadSymbols = hasSymbols, AssemblyResolver = assemblyResolver });

//            // do some weaving
//            foreach (var typeFullName in typesToProcess)
//            {
//                var typeDefinition = assembly.MainModule.GetType(typeFullName.Key);
//                foreach (var weaver in weavers)
//                {
//                    weaver.Weave(assembly, typeDefinition, typeFullName.Value);
//                }
//            }

//            assembly.Write(peFilePath);
//        }
//    }
//}