using System;
using System.Collections.Generic;
using System.Linq;

namespace K3.Locators {

    public interface ILocatorWithKey : ILocator {
        void Register(object obj, object key);
        T Locate<T>(object key);
    }

    public interface ILocator {
        void Register(object obj);
        void Unregister(object obj);
        T Locate<T>();
        IEnumerable<T> LocateAll<T>();
    }

    public static class LocatorFactory {
        public static ILocator CreateLocator() => new SimpleLocator();
    }

    class SimpleLocator : ILocator {

        HashSet<object> allItemsInLocator = new HashSet<object>();

        Collections.Multidict<Type, object> cache = new Collections.Multidict<Type, object>();

        public void Register(object obj) { 
            if (obj == null) return;

            if (allItemsInLocator.Add(obj)) {
                var key = obj.GetType();
                cache.Clear();
                //var baseTypes = ReflectionUtility.FindAllBaseTypes(key);
                //foreach (var t in baseTypes) {
                //    if (cache.Has(t)) cache.Add(t, obj);
                //}
            }
        }

        public void Unregister(object obj) {
            cache.Clear();
            // cache.RemoveValue(obj);
        }

        void BuildCacheIfNecessary<T>() {
            var key = typeof(T);
            if (cache.Has(key)) return;
            foreach (var item in allItemsInLocator) if (item is T titem) cache.Add(key, titem);
        }

        public T Locate<T>() {
            var key = typeof(T);
            BuildCacheIfNecessary<T>();
            return (T)cache.First(key);
        }

        public IEnumerable<T> LocateAll<T>() {
            var key = typeof(T);
            BuildCacheIfNecessary<T>();
            foreach (var item in cache.All(key)) yield return (T)item;
        }
    }
}