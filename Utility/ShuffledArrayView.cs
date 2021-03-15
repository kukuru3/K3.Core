using System.Collections.Generic;

using UnityEngine;

namespace K3 {
    static public partial class Randoms {
        public class ShuffledArrayView<T> {

            // for starters, naive approach that manipulates entire arrays of references
            // later, it might be cheaper to just juggle integer indices (since work with ints should be faster
            // than references - maybe??? refs are also just int32 numbers -- test!!!
            readonly T[] sourceArray;
            T[][] buckets;
            int numBuckets;

            int currentBucket;
            int currentIndex = int.MaxValue;

            public void Reset() {
                currentBucket = 0;
                currentIndex = 0;
                if (sourceArray.Length == 0) return;
                InitialDistribution();
                StartBucket(0);
            }

            private void StartBucket(int b) {
                currentBucket = b;
                buckets[b].ShuffleInPlace();
                currentIndex = 0;
            }

            private void NextBucket() {
                if (currentBucket + 1 >= numBuckets) StartBucket(0);
                else StartBucket(currentBucket + 1);
            }

            public ShuffledArrayView(T[] source) {
                sourceArray = source;
                Reset();
            }

            public T NextValue() {
                var val = buckets[currentBucket][currentIndex];
                currentIndex++;
                if (currentIndex >= buckets[currentBucket].Length) NextBucket();
                return val;
            }

            private void InitialDistribution() {
                numBuckets = Mathf.FloorToInt(Mathf.Log(sourceArray.Length) + 0.3f);

                var arr = new T[sourceArray.Length];
                sourceArray.CopyTo(arr, 0);
                arr.ShuffleInPlace();

                List<T>[] lists = new List<T>[numBuckets];
                for (var i = 0; i < numBuckets; i++) lists[i] = new List<T>();
                for (var i = 0; i < sourceArray.Length; i++) {
                    lists[i % numBuckets].Add(sourceArray[i]);
                }

                buckets = new T[numBuckets][];
                for (var i = 0; i < numBuckets; i++) buckets[i] = lists[i].ToArray();
            }
        }
    }
}