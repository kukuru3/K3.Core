using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace K3 {

    static public partial class Dice {
        
        /// <summary>Rolls a dice with results 1-100</summary>
        public static int D100 => 1+Random.Range(0, 100);
        
        public static int Successes(int dice, int threshold, int dieSides = 6) {
            var counter = 0;
            for (var i = 0; i < dice; i++)
                if (Random.Range(1, dieSides+1) >= threshold) counter++;
            return counter;
        }

        public static int Roll(int dice, int dieSides = 6) {
            var sum = 0;
            for (var i = 0; i < dice; i++) {
                sum += Random.Range(0, dieSides) + 1;
            }
            return sum;
        }

        public static bool Success(int threshold, int dieSides = 6) => Successes(1, threshold, dieSides) == 1;
        public static bool PercentChance(this int threshold) => Success(100-threshold, 100);

    }

    static public partial class Randoms {

        public static void ShuffleInPlace<T>(this T[] array) {
            int n = array.Length;
            while (n > 1) {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static float Range(float low, float high) => low + (high - low) * UnityEngine.Random.value;
        public static int Range(int low, int high) => low + Mathf.RoundToInt((high - low) * UnityEngine.Random.value);

        static public T PickRandom<T>(this IList<T> fromCollection) {
            if (fromCollection.Count == 0) return default;
            return fromCollection[UnityEngine.Random.Range(0, fromCollection.Count)];
        }

        static public float Deviate(this float mean, float? sigma = null) {
            if (!sigma.HasValue) sigma = Mathf.Abs(mean) * 0.3f;
            return mean + BoxMuller() * sigma.Value;
        }

        /// <summary> Since Box-Muller generates 2 independent uniform variables during execution, we can save 1 for the next call.</summary>
        static private float? boxMullerSpare;

        static private float BoxMuller() {

            if (boxMullerSpare != null) {
                var val = boxMullerSpare.Value;
                boxMullerSpare = null;
                return val;
            }
            float x, y, z;
            do {
                x = 2f * UnityEngine.Random.value - 1f;
                y = 2f * UnityEngine.Random.value - 1f;
                z = x * x + y * y;
            } while (z > 1f || z == 0f);

            var fac = Mathf.Sqrt(-2f * Mathf.Log(z) / z);
            boxMullerSpare = x * fac;
            return y * fac;
        }

        public static T Pick<T>(IEnumerable<(T item, float weight)> items) {
            // Calculate the total weight
            float totalWeight = items.Sum(x => x.weight);

                // Generate a random number between 0 and the total weight
            float randomNumber = UnityEngine.Random.Range(0f, totalWeight);

            // Iterate through the items
            float runningTotal = 0;
            foreach (var (item, weight) in items) {
                runningTotal += weight;
                if (randomNumber < runningTotal) return item;
            }

            // Fallback in case of rounding errors or empty collection
            throw new System.InvalidOperationException("Cannot pick an item from an empty or invalid collection.");
        }
    }
}