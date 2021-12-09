using System;
using System.Collections.Generic;
using System.Linq;

namespace K3 {
    public static class Enums {
        public static IEnumerable<T> IterateValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();

        public static int MaxValue<T>() => IterateValues<T>().Cast<int>().Max();

        public static TValue[] MapToArrayAndPopulate<TValue, TEnum>() where TValue : new() where TEnum : Enum {
            var arr = MapToArray<TValue, TEnum>();
            for (var i =0; i < arr.Length; i++) arr[i] = new TValue();
            return arr;
        }

        public static TValue[] MapToArrayAndPopulate<TValue, TEnum>(TValue defaultValue = default) where TValue : struct where TEnum : Enum{
            var arr = MapToArray<TValue, TEnum>();
            for (var i = 0; i < arr.Length; i++) arr[i] = defaultValue;
            return arr;
        }

        public static TValue[] MapToArray<TValue, TEnum>() {
            var mv = MaxValue<TEnum>();
            // magic number assumptions were made:
            if (mv > 128) throw new InvalidOperationException("This is meant to be used with `simple` ascending enums");
            var arr = new TValue[mv + 1];
            return arr;
        }

        // Given an enum member, will return the next enum member (wraps over the last member)
        public static T Cycle<T>(this T originalValue) where T : Enum {
            var values = Enum.GetValues(typeof(T));
            var index = Array.IndexOf(values, originalValue);
            if (index == -1) throw new InvalidOperationException($"Could not cycle enum value {originalValue}");
            index++;
            if (index >= values.Length) index = 0;
            return (T)values.GetValue(index);
        }
    }
}
