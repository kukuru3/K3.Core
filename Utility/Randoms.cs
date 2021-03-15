using System.Collections.Generic;

using UnityEngine;

namespace K3 {

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

        static public T PickRandom<T>(this IList<T> fromCollection) {
            if (fromCollection.Count == 0) return default;
            return fromCollection[UnityEngine.Random.Range(0, fromCollection.Count)];
        }

        static public float Deviate(float mean, float? sigma = null) {
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
    }
}