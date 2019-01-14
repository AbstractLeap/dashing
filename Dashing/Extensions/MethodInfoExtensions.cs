namespace Dashing.Extensions {
    using System;
    using System.Reflection;

    public static class MethodInfoExtensions {
        // freaky Jon Skeet code heavily adapted from
        // https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
        public static Func<Session, object, TOut> ConvertToWeakDelegate<TOut>(this MethodInfo method) {
            if (typeof(TOut) != method.ReturnType) { throw new ArgumentException($"Method returns {method.ReturnType} but you tried to return {typeof(TOut)}"); }

            // First fetch the generic form
            MethodInfo genericHelper = typeof(MethodInfoExtensions).GetMethod(nameof(MagicMethodHelper),
                                                    BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod
                      (typeof(Session), method.GetParameters()[0].ParameterType, method.ReturnType); 

            // Now call it. The null argument is because it’s a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<Session, object, TOut>)ret;
        }

        private static Func<TTarget, object, TReturn> MagicMethodHelper<TTarget, TParam, TReturn>(MethodInfo method)
            where TTarget : class {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TParam, TReturn> func = (Func<TTarget, TParam, TReturn>)method.CreateDelegate
                        (typeof(Func<TTarget, TParam, TReturn>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<TTarget, object, TReturn> ret = (target, param) => func(target, (TParam)param);
            return ret;
        }
    }
}