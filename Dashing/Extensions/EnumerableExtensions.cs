namespace Dashing.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Dashing.Configuration;

    public static class EnumerableExtensions {
        public static IList<IMap> OrderTopologically(this IEnumerable<IMap> maps) {
            // NOTE: In terms of efficiency this isn't the best. If it's ever used in a time critical piece of code
            // I'd suggest re-writing. However, in terms of being able to understand it it's pretty simple i.e.
            // iterate over each map, see if it's at the bottom of the tree (or if everything underneath is already mapped)
            // and then add in. Otherwise skip it and come back to later.

            var result = new List<IMap>();
            var skippedMaps = new List<IMap>();
            maps = maps.ToList();
            foreach (var map in maps) {
                // if already processed ignore
                if (result.Contains(map)) {
                    continue;
                }

                // if has collection properties or referenced by something else then skip
                if ((map.Columns.Any(c => c.Value.Relationship == RelationshipType.OneToMany && c.Value.ChildColumn.Map != map && !result.Contains(c.Value.ChildColumn.Map)))
                    || maps.Any(m => !result.Contains(m) &&  m.Columns.Any(c => c.Value.Relationship == RelationshipType.ManyToOne && c.Value.Type == map.Type))) {
                    skippedMaps.Add(map);
                    continue;
                }

                // otherwise we add it in and follow the tree
                AddToResult(map, result, skippedMaps, maps);
            }

            // fix up the skipped ones
            while (skippedMaps.Any()) {
                var map = skippedMaps.First();
                if (result.Contains(map)) {
                    skippedMaps.Remove(map);
                    continue;
                }

                if (!((map.Columns.Any(c => c.Value.Relationship == RelationshipType.OneToMany && c.Value.ChildColumn.Map != map && !result.Contains(c.Value.ChildColumn.Map)))
                    || maps.Any(m => !result.Contains(m) && m.Columns.Any(c => c.Value.Relationship == RelationshipType.ManyToOne && c.Value.Type == map.Type)))) {
                    AddToResult(map, result, skippedMaps, maps);
                    skippedMaps.Remove(map);
                    continue;
                }

                // otherwise, we can't add this now, let's move it to the end
                skippedMaps.Remove(map);
                skippedMaps.Add(map);
            }

            Debug.Assert(maps.Count() == result.Count, "These should match, definitely a bug if not");

            return result;
        }

        private static void AddToResult(IMap map, IList<IMap> result, IList<IMap> skippedMaps, IEnumerable<IMap> maps) {
            result.Add(map);
            
            // now check for all relationship properties
            foreach (var relatedMap in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne).Select(c => c.Value.ParentMap)) {
                // if already mapped ignore
                if (result.Contains(relatedMap)) {
                    // this should come after this map so let's move it
                    continue;
                }

                // if has collection properties that aren't the map ignore
                if ((relatedMap.Columns.Any(c => c.Value.Relationship == RelationshipType.OneToMany && c.Value.ChildColumn.Map != map && !result.Contains(c.Value.ChildColumn.Map)))
                    || maps.Any(m => !result.Contains(m) && m.Columns.Any(c => c.Value.Relationship == RelationshipType.ManyToOne && c.Value.Type == relatedMap.Type))) {
                    skippedMaps.Add(relatedMap);
                    continue;
                }

                AddToResult(relatedMap, result, skippedMaps, maps);
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source) {
            return !source.Any();
        }

        public static IEnumerable<IEnumerable<T>> Subsets<T>(this IEnumerable<T> source) {
            var list = source.ToList();
            int length = list.Count;
            var max = (int)Math.Pow(2, list.Count);

            for (int count = 0; count < max; count++) {
                var subset = new List<T>();
                uint rs = 0;
                while (rs < length) {
                    if ((count & (1u << (int)rs)) > 0) {
                        subset.Add(list[(int)rs]);
                    }

                    rs++;
                }

                yield return subset;
            }
        }
    }
}