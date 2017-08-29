//namespace Dashing.CodeGeneration.Weaving {
//    using System;
//    using System.Collections.Generic;

//    using Mono.Cecil;

//#if !NET451
        


//    public sealed class AssemblyResolutionException : Exception
//    {

//        readonly AssemblyNameReference reference;

//        public AssemblyNameReference AssemblyReference {
//            get { return reference; }
//        }

//        public AssemblyResolutionException(AssemblyNameReference reference)
//            : base(string.Format("Failed to resolve assembly: '{0}'", reference))
//        {
//            this.reference = reference;
//        }
//    }

//    class DotNetCoreAssemblyResolver : IAssemblyResolver
//    {
//        Dictionary<string, Lazy<AssemblyDefinition>> _libraries;

//        public DotNetCoreAssemblyResolver()
//        {
//            _libraries = new Dictionary<string, Lazy<AssemblyDefinition>>();

//            var compileLibraries = new DependencyContext();
//            foreach (var library in compileLibraries)
//            {
//                var path = library.ResolveReferencePaths().FirstOrDefault();
//                if (string.IsNullOrEmpty(path))
//                    continue;

//                _libraries.Add(library.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = this })));
//            }
//        }

//        public virtual AssemblyDefinition Resolve(string fullName)
//        {
//            return Resolve(fullName, new ReaderParameters());
//        }

//        public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
//        {
//            if (fullName == null)
//                throw new ArgumentNullException("fullName");

//            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
//        }

//        public AssemblyDefinition Resolve(AssemblyNameReference name)
//        {
//            return Resolve(name, new ReaderParameters());
//        }

//        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
//        {
//            if (name == null)
//                throw new ArgumentNullException("name");

//            Lazy<AssemblyDefinition> asm;
//            if (_libraries.TryGetValue(name.Name, out asm))
//                return asm.Value;

//            throw new AssemblyResolutionException(name);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposing)
//                return;

//            foreach (var lazy in _libraries.Values)
//            {
//                if (!lazy.IsValueCreated)
//                    continue;

//                lazy.Value.Dispose();
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//#endif
//}