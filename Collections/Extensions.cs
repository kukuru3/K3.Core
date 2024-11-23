using System.Collections.Generic;

namespace K3 {
    public static class CollectionExtensions {
        public static V GetOrCreate<K,V>(this Dictionary<K,V> dict, K key) where V: new() {
            if (!dict.TryGetValue(key, out var existing)) {
                existing = dict[key] = new V();
            }
            return existing;
        }
        public static V GetOrCreate<K,V>(this Dictionary<K,V> dict, K key, System.Func<V> ctor) {
            if (!dict.TryGetValue(key, out var existing)) {
                existing = dict[key] = ctor();
            }
            return existing;
        }
    }

    public static class CollectionUtilities {
        public static (ISet<T> added, ISet<T> removed) GetSetChanges<T>(IEnumerable<T> old, IEnumerable<T> @new) {
            var added = new HashSet<T>();
            var removed = new HashSet<T>();

            var oldSet = new HashSet<T>(old);
            var newSet = new HashSet<T>(@new);

            added.UnionWith(newSet); added.ExceptWith(oldSet);
            removed.UnionWith(oldSet); removed.ExceptWith(newSet);
            return (added, removed);
        }
    }
}