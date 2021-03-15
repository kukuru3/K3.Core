using System;

namespace K3 {
    public static class Convert {
        static readonly string[] byteSizeSuffixes = { " bytes", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        const float SPEED_OF_SOUND = 330;
        const float POWER_OF_A_HORSE = 745.7f;

        /// <summary> Converts into bytes, kilobytes, etc. as appropriate</summary>
        /// <param name="rawBytes">raw number of bytes to convert</param>
        /// <param name="decimalPlaces"></param>
        /// <returns>A nicely formated string, i.e. 31kB, 7.3MB etc</returns>
        static public string AsByteSize(long rawBytes, int decimalPlaces = 1) {
            if (decimalPlaces < 0) throw new ArgumentOutOfRangeException("decimalPlaces");
            if (rawBytes < 0) return "-" + AsByteSize(-rawBytes);
            if (rawBytes == 0) return $"0 bytes";

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(rawBytes, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)rawBytes / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000) {
                mag += 1;
                adjustedSize /= 1024;
            }
            return $"{adjustedSize:decimalPlaces}{byteSizeSuffixes[mag]}";
        }

        static public float ToMach(float metersPerSecond) {
            return metersPerSecond / SPEED_OF_SOUND;
        }

        static public float ToKmH(float metersPerSecond) {
            return metersPerSecond * 3.6f;
        }
        static public float HorsepowerToWatts(float horsepower) => horsepower * POWER_OF_A_HORSE;

        static public float WattsToHorsepower(float watts)  => watts / POWER_OF_A_HORSE;
        
    }
}
