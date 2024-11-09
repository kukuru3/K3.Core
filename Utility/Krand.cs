using System;

namespace K3 {
    
    public static class Cypher {
        // For security purposes, we don't want to name groups "Group 1", "Group 2..."
        // so we use a simple block cypher. It's literally just code some guy posted on StackOverflow
        // source: https://stackoverflow.com/questions/1971657/32-bit-block-cipher-for-net/9718378
        // but it's fast and I ran an automated test that detected no collisions so it seems it truly is a 1-1 block cypher.
        // and it's helluva faster to transmit than a GUID would ever be.
        public static uint UltraSimpleBlockCypher(uint key, uint data)
        {
            uint R = (data ^ key) & 0xFFFF, L = (data >> 16) ^ (((((R >> 5) ^ (R << 2)) + ((R >> 3) ^ (R << 4))) ^ ((R ^ 0x79b9) + R)) & 0xFFFF);
            key = (key >> 3) | (key << 29);
            R ^= ((((L >> 5) ^ (L << 2)) + ((L >> 3) ^ (L << 4))) ^ ((L ^ 0xf372) + (L ^ key))) & 0xFFFF;
            return ((L ^ ((((R >> 5) ^ (R << 2)) + ((R >> 3) ^ (R << 4))) ^ ((R ^ 0x6d2b) + (R ^ ((key >> 3) | (key << 29)))))) << 16) | R;
        }
    }

    public static class KrandUtils {
        public static ulong Squirrel3(ulong position, ulong seed = 0) {
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

        static readonly KRand defaultKrander = new KRand(DateTime.Now.GetHashCode());

        public static bool PctChance(this int threshold) => defaultKrander.Success(100-threshold, 100);
    }

    public class KRand {
        
        ulong position;
        ulong seed;

        public void Reset() {
            position = 0;
        }
        
        public KRand(int seed = 0) { unchecked {  this.seed = (ulong)(seed); } }
        public KRand(ulong seed) => this.seed = seed;
        public KRand(long seed) { unchecked {  this.seed = (ulong)(seed); } }

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