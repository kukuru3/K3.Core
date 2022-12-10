using UnityEngine;

namespace K3 {
    public static class Colors {
        public static float Luma(this Color color) {
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b; 
        }

        public static float GetRelativeLuminance(Color color, Color reference) {
            return Luma(color) / Luma(reference);
        }
    }
}
