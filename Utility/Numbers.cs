using UnityEngine;

namespace K3 {
    static public class Numbers {
        public static float Map(this float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo, bool constrained = true) {
            var t = (source - sourceFrom) / (sourceTo - sourceFrom);
            if (constrained) t = Mathf.Clamp01(t);
            return targetFrom + t * (targetTo - targetFrom);
        }

        public static int Map(this int source, int sourceFrom, int sourceTo, int targetFrom, int targetTo, bool constrained = true) {
            var t = (float)(source - sourceFrom) / (sourceTo - sourceFrom);
            if (constrained) t = Mathf.Clamp01(t);
            return Mathf.RoundToInt(targetFrom + t * (targetTo - targetFrom));
        }

        public static float[] QuadraticEquation(float termA, float termB, float termC) {
            // sanity checks:
            if (Mathf.Approximately(termA, 0f)) {
                if (Mathf.Approximately(termB, 0f)) /* nonsense equation 0x^2 + 0x +... */ return new float[0] { };
                else /* linear equation */ return new[] { -termC / termB };
            }

            var p = -termB / (2 * termA);
            var inRoot = termB * termB - 4 * termA * termC;

            if (inRoot < 0f) return new float[0] { };

            var q = Mathf.Sqrt(inRoot) / (2 * termA);

            if (Mathf.Approximately(q, 0)) return new[] { p };
            else return new[] { p + q, p - q };
        }

        public static float? SmallestPositive(this float[] source) {
            float? result = null;
            foreach (var item in source) if (result == null || (item < result && item > 0f)) result = item;
            return result;
        }
    }
}
