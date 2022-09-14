using System;
using System.Net.WebSockets;

namespace K3 {
    
    static class KrandUtils {
        internal static ulong Squirrel3(ulong position, ulong seed = 0) {
            const ulong BIT_NOISE1 = 0xB5297A4DB5297A4D;
            const ulong BIT_NOISE2 = 0x68E31DA468E31DA4;
            const ulong BIT_NOISE3 = 0x1B56C4E91B56C4E9;
    
            ulong mangled = position;
            unchecked { 
                mangled *= BIT_NOISE1;
                mangled += seed;
                mangled ^= (mangled >> 8);
                mangled += BIT_NOISE2;
                mangled ^= (mangled << 8);
                mangled *= BIT_NOISE3;
                mangled ^= (mangled >> 8);
            }
            return mangled;
        }

        static KRand defaultKrander = new KRand(DateTime.Now.GetHashCode());

        public static bool PercentChance(this int threshold) => defaultKrander.Success(100-threshold, 100);
    }

    public class KRand {
        
        ulong position;
        ulong seed;

        public void Reset() {
            position = 0;
        }
        
        public KRand(int seed = 0) {
            unchecked { 
                this.seed = (ulong)(seed);
            }
        }
        
        ulong _NextUlong() {
            return KrandUtils.Squirrel3(position++, seed);
        }
        
        int NextInt() {
            unchecked { 
                return (int)(_NextUlong() >> 32);
            }
        }
        double _NextDbl() {
            return _NextUlong() * (1.0/ulong.MaxValue);
        }
        
        float _NextFlt() {
            return _NextUlong() * (1.0f/ulong.MaxValue);
        }
        
        /// <param name="from">INCLUSIVE from</param>
        /// <param name="to">INCLUSIVE to</param>
        public int Next(int from, int to) {
            var span = to - from;
            if (span > 0) 
                return from + NextInt() % (span+1);
            else if (span < 0) {
                return to + NextInt() % (1-span);
            }
            return from;                     
        }

        public float Next(float from, float to) {
            return from + (to - from) * _NextFlt();
        }

        public int Successes(int numDice, int threshold, int dieSides = 6) {
            var counter = 0;
            for (var i = 0; i < numDice; i++) {
                if (Next(1,dieSides) >= threshold) counter++;
            }
            return counter;   
        }

        public bool Success(int threshold, int dieSides = 6) => Successes(1, threshold, dieSides) == 1;

        public int Roll(int numDice, int dieSides = 6) {
            var sum = 0; 
            for (var i = 0; i < numDice; i++) sum += Next(1, dieSides);
            return sum;
        }
    }
}