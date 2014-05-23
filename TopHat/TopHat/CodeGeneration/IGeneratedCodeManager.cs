using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGeneration
{
    public interface IGeneratedCodeManager
    {
        /// <summary>
        /// Returns a reference to the generated code assembly
        /// </summary>
        Assembly GeneratedCodeAssembly { get; }

        /// <summary>
        /// Loads the Generated code for this project
        /// </summary>
        /// <param name="config"></param>
        /// <remarks>This method does not generate code nor check for updates</remarks>
        void LoadCode(CodeGeneratorConfig config);

        /// <summary>
        /// Returns a type for a Foreign Key class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type GetForeignKeyType<T>();

        /// <summary>
        /// Returns a type for a Tracked class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type GetTrackingType<T>();

        /// <summary>
        /// Returns an instance of a Foreign Key class for the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateForeignKeyInstance<T>();

        /// <summary>
        /// Returns an instance of a tracking class for the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateTrackingInstance<T>();

        /// <summary>
        /// Convenience method for enabling tracking on a tracking instance
        /// </summary>
        void TrackInstance<T>(T entity);
    }
}