using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace K3.Collections {

    // todo: needs finishin'
    public class ManyToManyWithData<Ta, Tb, TRelationData> : ManyToMany<Ta, Tb> where Ta : class where Tb : class {
        private readonly CreateData dataConstructor;

        public delegate TRelationData CreateData(Ta a, Tb b);

        Dictionary<CombinedKey, TRelationData> data = new();


        public ManyToManyWithData(bool preserveOrder, bool indexFastLookups, CreateData dataConstructor = null) : base(preserveOrder, indexFastLookups) {
            this.dataConstructor = dataConstructor;
        }

        class CombinedKey {
            internal readonly Ta a;
            internal readonly Tb b;

            public CombinedKey(Ta a, Tb b) {
                this.a = a;
                this.b = b;
            }

            public override int GetHashCode() => HashCode.Combine(a, b);
            public override bool Equals(object obj) {
                return obj is CombinedKey other && other.a == a && other.b == b;
            }
        }

        CombinedKey Key(Ta a, Tb b) => new(a, b);

        public bool Connect(Ta a, Tb b, TRelationData data) {
            var connected = base.Connect(a, b);
            if (connected) this.data.Add(Key(a,b), data);
            return connected;
        }

        public override bool Connect(Ta a, Tb b) {
            if (dataConstructor == null) throw new System.Exception("Either supply a data constructor when creating this relation, or call Connect with data provided");
            return Connect(a, b, dataConstructor(a,b));
        }

        public override bool Disconnect(Ta a, Tb b) {
            data.Remove(Key(a,b));
            return base.Disconnect(a, b);
        }

        public override void Clear() {
            base.Clear();
            data.Clear();
        }

        public override int Excise(Ta a) {
            var copyOfKeys = data.Keys.ToList();
            foreach (var item in copyOfKeys) if (item.a == a) data.Remove(item);
            return base.Excise(a);
        }

        public override int Excise(Tb b) {
            var copyOfKeys = data.Keys.ToList();
            foreach (var item in copyOfKeys) if (item.b == b) data.Remove(item);
            return base.Excise(b);
        }

        public new IEnumerable<(Ta relatedLeft, Tb relatedRight, TRelationData relationData)> ListAll() {
            foreach (var item in base.ListAll()) yield return (item.Item1, item.Item2, data[Key(item.Item1, item.Item2)]);
        }

        public TRelationData GetData(Ta a, Tb b) {
            if (!AreRelated(a,b)) return default;
            return data[Key(a,b)];
        }

        public new IEnumerable<(Tb relatedElement, TRelationData relationData)> ListRelations(Ta a) {
            foreach (var r in base.ListRelations(a)) yield return (r, data[Key(a, r)]);
        }
        public new IEnumerable<(Ta relatedElement, TRelationData relationData)> ListRelations(Tb b) {
            foreach (var r in base.ListRelations(b)) yield return (r, data[Key(r,b)]);
        }
    }

    public class ManyToMany<Ta, Tb> where Ta : class 
                                    where Tb : class {


        public ManyToMany(bool preserveOrder, bool indexFastLookups)
        {
            this.preserveOrder = preserveOrder;
            this.indexFastLookups = indexFastLookups;
        }


        class Collection<T> : IEnumerable<T> {
            public Collection(bool hasList, bool hasHashSet)
            {
                if (!hasList && !hasHashSet) hasList = true;
                if (hasList) _list = new List<T>();
                if (hasHashSet) _set = new HashSet<T>();
            }

            internal List<T> _list;
            internal HashSet<T> _set;

            internal bool Contains(T item) => _set?.Contains(item) ?? _list.Contains(item);
            internal bool Remove(T item) {
                var result = false ;
                if (_set != null) result |= _set.Remove(item);
                if (_list != null) result |= _list.Remove(item);
                return result;
            }

            internal void Add(T item) {
                if (_set != null) _set.Add(item);
                if (_list != null) _list.Add(item);
            }

            internal IEnumerable<T> GetElements() { if (_list != null) return _list; else return _set; }

            public IEnumerator<T> GetEnumerator() {
                if (_list != null) return _list.GetEnumerator();
                else return _set.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return ((IEnumerable)_list ?? (IEnumerable)_set).GetEnumerator();
            }
        }

        readonly Dictionary<Ta, Collection<Tb>> leftToRight = new();
        readonly Dictionary<Tb, Collection<Ta>> rightToLeft = new();
        private readonly bool preserveOrder;
        private readonly bool indexFastLookups;

        public virtual bool Connect(Ta a, Tb b) {
            var left = GetRelationsAtoB(a);
            var right = GetRelationsBtoA(b);
            if (left.Contains(b) || right.Contains(a)) return false;
            left.Add(b);
            right.Add(a);
            return true;
        }
        public virtual bool Disconnect(Ta a, Tb b) {
            return GetRelationsBtoA(b).Remove(a) && GetRelationsAtoB(a).Remove(b);
        }

        public virtual int Excise(Ta a) {
            int counter = 0;
            var atob = GetRelationsAtoB(a);
            foreach (var rel in atob) { 
                GetRelationsBtoA(rel).Remove(a);
                counter++;
            }
            leftToRight.Remove(a);
            return counter;
        }

        /// <summary>Removes all connections that have anything to do with the given element.</summary>        
        public virtual int Excise(Tb b) {
            int counter = 0;
            var btoa = GetRelationsBtoA(b);
            foreach (var rel in btoa) { 
                 GetRelationsAtoB(rel).Remove(b);
                counter++;
            }
            rightToLeft.Remove(b);
            return counter;
        }

        public virtual void Clear() {
            leftToRight.Clear();
            rightToLeft.Clear();
        }

        Collection<Tb> GetRelationsAtoB(Ta a) {
            if (!leftToRight.ContainsKey(a)) leftToRight[a] = new Collection<Tb>(preserveOrder, indexFastLookups);
            return leftToRight[a];
        }

        Collection<Ta> GetRelationsBtoA(Tb b) {
            if (!rightToLeft.ContainsKey(b)) rightToLeft[b] = new Collection<Ta>(preserveOrder, indexFastLookups);
            return rightToLeft[b];
        }

        protected bool AreRelated(Ta a, Tb b) {
            return GetRelationsAtoB(a).Contains(b);
        }

        

        public IEnumerable<(Ta, Tb)> ListAll() {
            foreach (var kvp in leftToRight) foreach (var b in kvp.Value) yield return (kvp.Key, b);
        }
        public IEnumerable<Tb> ListRelations(Ta a) => GetRelationsAtoB(a);
        public IEnumerable<Ta> ListRelations(Tb b) => GetRelationsBtoA(b);
    }
}
