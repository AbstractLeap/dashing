namespace Dashing.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Dashing.Configuration;

    public static class EnumerableExtensions {
        /// <remarks>
        /// In terms of efficiency this isn't the best. If it's ever used in a time critical piece of code
        /// I'd suggest re-writing. However, in terms of being able to understand it it's pretty simple i.e.
        /// iterate over each map, see if it's at the bottom of the tree (or if everything underneath is already mapped)
        /// and then add in. Otherwise skip it and come back to later.
        /// </remarks>
        public static TopologicalOrderResult OrderTopologically(this IEnumerable<IMap> enumerableOfMaps) {
            var result = new TopologicalOrderResult();

            // start by finding self referencing stuff
            var maps = enumerableOfMaps as List<IMap> ?? enumerableOfMaps.ToList();
            result.SelfReferencingMaps = maps.Where(m => m.Columns.Any(c => c.Value.Type == c.Value.Map.Type)).ToArray();
            result.OneToOneMaps = maps.Where(m => m.Columns.Any(c => c.Value.Relationship == RelationshipType.OneToOne && c.Value.Type != c.Value.Map.Type)).ToArray();

            var resultHash = new HashSet<IMap>();
            var resultList = new List<IMap>();
            var skippedMapsHash = new HashSet<IMap>();
            var skippedMapsList = new List<IMap>();
            foreach (var map in maps.Where(map => !resultHash.Contains(map))) {
                // if has collection properties or referenced by something else then skip
                if (HasNonOrderedReference(maps, resultHash, map)) {
                    AddMapToSkipped(map, skippedMapsList, skippedMapsHash);
                    continue;
                }

                // otherwise we add it in and follow the tree
                AddToResult(map, resultHash, resultList, skippedMapsHash, skippedMapsList, maps);
            }

            // fix up the skipped ones
            while (skippedMapsList.Any()) {
                var map = skippedMapsList.First();
                if (resultHash.Contains(map)) {
                    RemoveMapFromSkipped(map, skippedMapsList, skippedMapsHash);
                    continue;
                }

                if (!HasNonOrderedReference(maps, resultHash, map)) {
                    AddToResult(map, resultHash, resultList, skippedMapsHash, skippedMapsList, maps);
                    RemoveMapFromSkipped(map, skippedMapsList, skippedMapsHash);
                    continue;
                }
                else {
                    // check to see if it's one to one and the only remaining reference is from the one to one
                    if (result.OneToOneMaps.Contains(map) && !HasNonOrderedNonOneToOneReference(maps, resultHash, map)) {
                        AddToResult(map, resultHash, resultList, skippedMapsHash, skippedMapsList, maps);
                        RemoveMapFromSkipped(map, skippedMapsList, skippedMapsHash);
                        continue;
                    }
                }

                // otherwise, we can't add this now, let's move it to the end
                RemoveMapFromSkipped(map, skippedMapsList, skippedMapsHash);
                AddMapToSkipped(map, skippedMapsList, skippedMapsHash);
            }

            result.OrderedMaps = resultHash;
            return result;
        }

        private static void AddMapToSkipped(IMap map, IList<IMap> skippedMapsList, HashSet<IMap> skippedMapsHash) {
            if (!skippedMapsHash.Contains(map)) {
                skippedMapsList.Add(map);
                skippedMapsHash.Add(map);
            }
        }

        private static void RemoveMapFromSkipped(IMap map, IList<IMap> skippedMapsList, HashSet<IMap> skippedMapsHash) {
            if (skippedMapsHash.Contains(map)) {
                skippedMapsList.Remove(map);
                skippedMapsHash.Remove(map);
            }
        }

        private static bool HasNonOrderedReference(List<IMap> maps, HashSet<IMap> resultHash, IMap map) {
            return maps.Any(m => 
                m.Type != map.Type 
                && !resultHash.Contains(m) 
                && m.Columns.Any(c => (c.Value.Relationship == RelationshipType.ManyToOne || c.Value.Relationship == RelationshipType.OneToOne) && c.Value.Type == map.Type));
        }

        private static bool HasNonOrderedNonOneToOneReference(List<IMap> maps, HashSet<IMap> resultHash, IMap map) {
            return maps.Any(m =>
                m.Type != map.Type
                && !resultHash.Contains(m)
                && m.Columns.Any(c => c.Value.Relationship == RelationshipType.ManyToOne && c.Value.Type == map.Type));
        }

        private static void AddToResult(IMap map, HashSet<IMap> resultHash, List<IMap> resultList, HashSet<IMap> skippedMapsHash, IList<IMap> skippedMapsList, List<IMap> maps) {
            resultHash.Add(map);
            resultList.Add(map);

            // now check for all relationship properties
            foreach (var relatedMap in map.Columns.Where(c => c.Value.Relationship == RelationshipType.ManyToOne && c.Value.Type != map.Type).Select(c => c.Value.ParentMap).Where(relatedMap => !resultHash.Contains(relatedMap))) {
                // if has collection properties that aren't the map ignore
                if (HasNonOrderedReference(maps, resultHash, relatedMap)) {
                    AddMapToSkipped(relatedMap, skippedMapsList, skippedMapsHash);
                    continue;
                }

                AddToResult(relatedMap, resultHash, resultList, skippedMapsHash, skippedMapsList, maps);
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