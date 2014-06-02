namespace TopHat.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions {
        public static bool IsEmpty<T>(this IEnumerable<T> source) {
            return !source.Any();
        }

        public static IEnumerable<IEnumerable<T>> Subsets<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            int length = list.Count;
            var max = (int)Math.Pow(2, list.Count);

            for (int count = 0; count < max; count++)
            {
                var subset = new List<T>();
                uint rs = 0;
                while (rs < length)
                {
                    if ((count & (1u << (int)rs)) > 0)
                    {
                        subset.Add(list[(int)rs]);
                    }

                    rs++;
                }

                yield return subset;
            }
        }
    }
}