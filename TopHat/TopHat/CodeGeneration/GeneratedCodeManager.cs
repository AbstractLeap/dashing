namespace TopHat.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class GeneratedCodeManager : IGeneratedCodeManager {
        private Assembly generatedCodeAssembly;

        private IDictionary<Type, Type> foreignKeyTypes;

        private IDictionary<Type, Type> trackingTypes;

        public Assembly GeneratedCodeAssembly {
            get {
                if (this.generatedCodeAssembly == null) {
                    throw new NullReferenceException("You must load the code before you can access the assembly");
                }

                return this.generatedCodeAssembly;
            }
        }

        public void LoadCode(CodeGeneratorConfig config) {
            this.generatedCodeAssembly = Assembly.LoadFrom(config.Namespace + ".dll");

            // go through the defined types and add them
            this.foreignKeyTypes = new Dictionary<Type, Type>();
            this.trackingTypes = new Dictionary<Type, Type>();

            foreach (var type in this.generatedCodeAssembly.DefinedTypes) {
                // find the base type from the users code
                if (type.Name.EndsWith(config.ForeignKeyAccessClassSuffix)) {
                    this.foreignKeyTypes.Add(type.BaseType, type);
                }
                else if (type.Name.EndsWith(config.TrackedClassSuffix)) {
                    this.trackingTypes.Add(type.BaseType.BaseType, type); // tracking classes extend fkClasses
                }
            }
        }

        public Type GetForeignKeyType<T>() {
            return this.foreignKeyTypes[typeof(T)];
        }

        public Type GetTrackingType<T>() {
            return this.trackingTypes[typeof(T)];
        }

        public T CreateForeignKeyInstance<T>() {
            return (T)Activator.CreateInstance(this.GetForeignKeyType<T>());
        }

        public T CreateTrackingInstance<T>() {
            return (T)Activator.CreateInstance(this.GetTrackingType<T>());
        }

        public void TrackInstance<T>(T entity) {
            ITrackedEntityInspector<T> inspector = new TrackedEntityInspector<T>(entity);
            inspector.ResumeTracking();
        }
    }
}