using System.Collections.Generic;

namespace K3.Collections {
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