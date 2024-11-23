using System.Collections.Generic;

namespace K3.Collections {
    public class Multidict<TKey, TValue> {

        Dictionary<TKey, List<TValue>> lists = new Dictionary<TKey, List<TValue>>();

        List<TValue> GetList(TKey key, bool create = false) {
            if (!lists.TryGetValue(key, out var result) && create) result = lists[key] = new List<TValue>();
            return result;
        }

        public void Add(TKey key, TValue value) {
            GetList(key, true).Add(value);
        }

        public void AddMany(TKey key, IEnumerable<TValue> values) {
            var list = GetList(key, true);
            list.AddRange(values);
        }

        public TValue First(TKey key) {
            var list = GetList(key);
            if (list == null) return default;
            if (list.Count > 0) return list[0];
            return default;
        }

        public IReadOnlyList<TValue> All(TKey key) => GetList(key, true);

        public IReadOnlyList<TValue> TryGetAll(TKey key) => GetList(key, false);

        public bool Has(TKey key) => lists.ContainsKey(key);

        /// <summary> Removes value from all lists. Also removes multiples</summary>
        /// <param name="val">Value to remove</param>
        /// <returns> Number of removed items. </returns>
        public int RemoveValue(TValue val) {
            int counter = 0;
            foreach (var list in lists.Values) if (list.Remove(val)) counter++;
            return counter;
        }

        /// <summary> Removes <paramref name="key"/> and all values associated with it </summary>
        public void Remove(TKey key) {
            lists.Remove(key);
        }

        public void RemoveExact(TKey key, TValue val) {
            GetList(key)?.Remove(val);
        }

        public void Clear() {
            lists.Clear();
        }

        public IEnumerable<TKey> AllKeys() {
            foreach (var item in lists) yield return item.Key;
        }

        public IEnumerable<TValue> AllValues() {
            foreach (var list in lists.Values) foreach (var item in list) yield return item;
        }
    }
}