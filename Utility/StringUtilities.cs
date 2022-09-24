using System.Text.RegularExpressions;

namespace K3 {
    public static class StringUtilities {
        public static bool WildcardMatch(this string str, string wildcardString) {
            return Regex.IsMatch(str, LikeToRegular(wildcardString));
        }

        private static string LikeToRegular(string value) {
            value = value
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("*", ".+");

            return $"^{value}$";
        }
    }
}