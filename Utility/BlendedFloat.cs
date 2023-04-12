using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

namespace K3.Utility {
    // consider: per-layer controls, such as:
    // - normalise weights inside layer (effectively, replaces the previous layers)
    // - allow a layer to limit or otherwise gate the variable as it passes through

    /// <summary>
    /// Meant to replace old FloatGate. 
    /// Complex Controlled value has layers of influence, and blend weights.
    /// All "blend" items in a single layer will blend together according to their weights.
    /// weights within a single layer are normalized, however, the maximum of those weights is used to blend the layer onto previous layers.
    /// </summary>
    public class BlendedFloat {

        struct Record {
            internal readonly int tokenIndex;
            internal readonly int layer;
            internal float weight;
            internal float value;

            public Record(int globalTokenIndex, int layer) {
                this.layer = layer;
                tokenIndex = globalTokenIndex;
                weight = default;
                value = default;
            }
        }

        List<int> layerIndices = new List<int>();

        bool _layerIndicesNeedSorting;
        bool _valueNeedsUpdating;
        float _currentValue;

        // converts from global token index to local token index
        Dictionary<int, int> tokenMapping = new();
        List<Record> records = new();

        public AccessToken ObtainAccessToken(int layerIndex) {
            var token = TokenRepository.GetNewToken(this);
            var r = new Record(token._tokenIndex, layerIndex);
            tokenMapping.Add(token._tokenIndex, records.Count);
            records.Add(r);
            if (!layerIndices.Contains(layerIndex)) {
                layerIndices.Add(layerIndex);
                _layerIndicesNeedSorting = true;
            }
            return token;
        }

        public void Set(ref AccessToken thandle, float value, float weight = 1f) {
            var l = tokenMapping[thandle._tokenIndex];
            var record = records[l]; // local index
            record.value = value;
            record.weight = Mathf.Clamp01(weight);

            records[l] = record;
            _valueNeedsUpdating = true;
        }

        public float GetValue() {
            EnsureValueComputed();
            return _currentValue;
        }

        float ComputeValue() {
            EnsureListSorted();

            var result = 0f;

            for (var li = 0; li < layerIndices.Count; li++) {
                var l = layerIndices[li];

                // start for layer
                var numMembers = 0;
                var weightSum = 0f;
                var weightMax = 0f;
                var valueSum = 0f;

                foreach (var r in records) if (r.layer == l) {
                    numMembers++;
                    weightSum += r.weight;
                    weightMax = Mathf.Max(weightMax, r.weight);
                    valueSum += r.value * r.weight;
                }

                if (weightSum > 0f) { 
                    var layerValue = valueSum / weightSum;
                    var layerWeightInBlend = Mathf.Min(weightMax, 1f);
                    result = Mathf.Lerp(result, layerValue, (li == 0) ? 1f : layerWeightInBlend);
                }
            }

            return result;
        }

        private void EnsureValueComputed() {
            if (_valueNeedsUpdating) {
                _valueNeedsUpdating = false;
                _currentValue = ComputeValue();
            }
        }

        private void EnsureListSorted() {
            if (_layerIndicesNeedSorting) {
                _layerIndicesNeedSorting = false; 
                layerIndices.Sort();
            }
        }
    }   

    public interface IBlendable {

    }

    internal struct TokenRepository {
        static List<AccessToken> tokens = new();

        static internal AccessToken GetNewToken(BlendedFloat owner) {
            var t = new AccessToken(tokens.Count, owner);
            tokens.Add(t);
            return t;
        }
    }

    public struct AccessToken {
        internal readonly int _tokenIndex;
        internal readonly BlendedFloat owner;
        public bool IsInitialized() => owner != null;

        public AccessToken(int index, BlendedFloat owner) {
            _tokenIndex = index;
            this.owner = owner;
        }

        public void Set(float value, float weight = 1f) {
            if (owner == null) throw new System.Exception($"owner null - token not initialized");
            owner.Set(ref this, value, weight);
        }
    }
    
}
