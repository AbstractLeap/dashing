namespace Dashing.Extensions {
    using System;
    using System.Reflection;

    public static class MethodInfoExtensions {
        // freaky Jon Skeet code adapted from
        // https://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
        public static Func<T, object, object> ConvertToWeakDelegate<T>(this MethodInfo method)
            where T : class {
            // First fetch the generic form
            MethodInfo genericHelper = typeof(Session).GetMethod(nameof(MagicMethodHelper),
                                                    BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod
                      (typeof(T), method.GetParameters()[0].ParameterType, method.ReturnType); 

            // Now call it. The null argument is because it’s a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<T, object, object>)ret;
        }

        private static Func<TTarget, object, object> MagicMethodHelper<TTarget, TParam, TReturn>(MethodInfo method)
            where TTarget : class {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TParam, TReturn> func = (Func<TTarget, TParam, TReturn>)method.CreateDelegate
                        (typeof(Func<TTarget, TParam, TReturn>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<TTarget, object, object> ret = (target, param) => func(target, (TParam)param);
            return ret;
        }
    }
}