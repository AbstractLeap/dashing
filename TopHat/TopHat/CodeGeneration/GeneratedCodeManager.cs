using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGeneration
{
    public class GeneratedCodeManager : IGeneratedCodeManager
    {
        private Assembly generatedCodeAssembly;

        private IDictionary<Type, Type> fkTypes;

        private IDictionary<Type, Type> trackingTypes;

        public Assembly GeneratedCodeAssembly
        {
            get
            {
                if (this.generatedCodeAssembly == null)
                {
                    throw new NullReferenceException("You must load the code before you can access the assembly");
                }
                return this.generatedCodeAssembly;
            }
        }

        public void LoadCode(CodeGeneratorConfig config)
        {
            this.generatedCodeAssembly = Assembly.LoadFrom(config.Namespace + ".dll");

            // go through the defined types and add them
            this.fkTypes = new Dictionary<Type, Type>();
            this.trackingTypes = new Dictionary<Type, Type>();

            foreach (var type in this.generatedCodeAssembly.DefinedTypes)
            {
                // find the base type from the users code
                if (type.Name.EndsWith(config.ForeignKeyAccessClassSuffix))
                {
                    this.fkTypes.Add(type.BaseType, type);
                }
                else
                {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                }
            }
        }

        public Type GetForeignKeyType<T>()
        {
            return this.fkTypes[typeof(T)];
        }

        public Type GetTrackingType<T>()
        {
            return this.trackingTypes[typeof(T)];
        }

        public T CreateForeignKeyInstance<T>()
        {
            return (T)Activator.CreateInstance(this.GetForeignKeyType<T>());
        }

        public T CreateTrackingInstance<T>()
        {
            return (T)Activator.CreateInstance(this.GetTrackingType<T>());
        }

        public void TrackInstance<T>(T entity)
        {
            ITrackedEntityInspector<T> inspector = new TrackedEntityInspector<T>(entity);
            inspector.ResumeTracking();
        }
    }
}