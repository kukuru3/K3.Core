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
}