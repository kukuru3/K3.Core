using System;
using System.Collections.Generic;

using UnityEngine;

using int2 = UnityEngine.Vector2Int;

namespace K3.Utility {

    public interface IHas3dPosition {
        Vector3 Position { get; }
    }

    public interface IHasMapPosition {
        Vector2 MapPosition { get; }
    }

    /// <summary>Optimized for 2d-like games, where a transform's Y coordinate is ignored.</summary>
    public class SimpleSpatialHash<T> {
        private class Cell {
            public Cell(int2 key) {
                this.key = key;
                items = new HashSet<T>();
            }
            public readonly int2 key;
            public HashSet<T> items;
        }

        Dictionary<T, int2> objToKeyLookup = new Dictionary<T, int2>();
        Dictionary<int2, Cell> sparseGrid = new Dictionary<int2, Cell>();
        float _cellResolution = 10f;

        public SimpleSpatialHash() {
            if (typeof(IHas3dPosition).IsAssignableFrom(typeof(T))) positionEvaluator = GetPositionFromInterface;
            else if (typeof(IHasMapPosition).IsAssignableFrom(typeof(T))) positionEvaluator = GetPositionFromInterface2D;
            else if (typeof(Component).IsAssignableFrom(typeof(T))) positionEvaluator = GetPositionFromComponent;
            else throw new Exception($"Cannot use spatial hashing on {typeof(T)} since there is no way to extract position from it.");
        }

        public IEnumerable<T> AllItems() => objToKeyLookup.Keys;

        Vector2 GetPositionFromComponent(T obj) => (obj as Component).transform.position.Flatten();
        Vector2 GetPositionFromInterface(T hasPos) => (hasPos as IHas3dPosition).Position.Flatten();
        Vector2 GetPositionFromInterface2D(T hasPos) => (hasPos as IHasMapPosition).MapPosition;

        delegate Vector2 GetPositionDelegate(T sourceObject);

        GetPositionDelegate positionEvaluator;

        // you could concievably also have it automatically scale resolution based on the maximum and minimum of the items, and the number of items.
        public void ChangeCellResolution(float newResolution) {
            this._cellResolution = newResolution;
            var fullSet = new HashSet<T>();
            foreach (var cell in sparseGrid.Values) fullSet.UnionWith(cell.items);
            sparseGrid.Clear();
            objToKeyLookup.Clear();
            foreach (var item in fullSet) Add(item);
        }

        Cell GetCell(int2 atKey) {
            if (!sparseGrid.TryGetValue(atKey, out var cell)) {
                cell = new Cell(atKey);
                sparseGrid.Add(atKey, cell);
            }
            return cell;
        }

        int2 GetKey(T @object) { 
            return GetKey(positionEvaluator(@object));
        }

        int2 GetKey(Vector2 position) => new int2(Mathf.FloorToInt(position.x / _cellResolution), Mathf.FloorToInt(position.y / _cellResolution));

        public void Add(T @object) {
            var key = GetKey(@object);
            var cell = GetCell(key);
            cell.items.Add(@object);
            objToKeyLookup.Add(@object, key);
        }

        public void Update(T @object) {
            var newKey = GetKey(@object);
            var existingKey = objToKeyLookup[@object];
            if (newKey != existingKey) {
                GetCell(existingKey).items.Remove(@object);
                GetCell(newKey).items.Add(@object);
                objToKeyLookup[@object] = newKey;
            }
        }

        public void AddOrUpdate(T @object) {
            if (objToKeyLookup.TryGetValue(@object, out var key)) Update(@object); else Add(@object);
        }

        public void Remove(T @object) {
            var key = objToKeyLookup[@object];
            GetCell(key).items.Remove(@object);
            objToKeyLookup.Remove(@object); 
        }

        public IEnumerable<T> GetObjectsInRadius(Vector2 worldCenter, float radius) {
            // this is a naive "in range" check. "Intersect circle with grid" would do better.
            // as is, we check up to 27% more grid squares than necessary
            var minX = Mathf.FloorToInt((worldCenter.x - radius) / _cellResolution);
            var maxX = Mathf.FloorToInt((worldCenter.x + radius) / _cellResolution);
            var minY = Mathf.FloorToInt((worldCenter.y - radius) / _cellResolution);
            var maxY = Mathf.FloorToInt((worldCenter.y + radius) / _cellResolution);
            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++) {
                if (sparseGrid.TryGetValue(new int2(x,y), out var cell)) {
                    foreach (var item in cell.items) {
                        if (Vector2.SqrMagnitude(positionEvaluator(item) - worldCenter) <= radius * radius) yield return item;
                    }
                }
            }
        }
    }
}