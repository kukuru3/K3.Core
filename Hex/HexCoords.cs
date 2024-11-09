using System;
using UnityEngine;

namespace K3.Hex {
    [System.Serializable]
    public struct Hex {

        public readonly int q;
        public readonly int r;
        public readonly int S => -q - r;

        public Hex(int q, int r) {
            this.q = q;
            this.r = r;
        }

        public static Hex operator +(Hex a, Hex b) => new Hex(a.q + b.q, a.r + b.r);
        public static Hex operator -(Hex a, Hex b) => new Hex(a.q - b.q, a.r - b.r);

        public static bool operator ==(Hex a, Hex b) => a.q == b.q && a.r == b.r;
        public static bool operator !=(Hex a, Hex b) => a.q != b.q || a.r != b.r;

        public void Deconstruct(out int Q, out int R) { Q = q; R = r; }
        public void Deconstruct(out int Q, out int R, out int S) { Q = q; R = r; S = this.S; }

        public static implicit operator Hex((int q, int r) t) => new Hex(t.q, t.r);

        public static int Distance(Hex a, Hex b) {
            var delta = a - b;
            return Math.Abs(delta.q) + Math.Abs(delta.r) + Math.Abs(delta.S) / 2;
        }

        public int DistanceTo(Hex b) => Distance(this, b);
        public override bool Equals(object other) => other is Hex hex && q == hex.q && r == hex.r;
        public override int GetHashCode() => HashCode.Combine(q, r);

        public override string ToString() => $"{q},{r},{S}";
    }

    public enum OffsetSystems {
        /// <summary>Pointy top, odd rows shifted by +½ column </summary>
        OddR,
        /// <summary>Pointy top, even rows shifted by +½ column </summary>
        EvenR,
        /// <summary> Flat top, odd columns shifted by +½ row</summary>
        OddQ, 
        /// <summary> Flat top, even columns shifted by +½ row</summary>
        EvenQ
    }

    public enum GridTypes {
        PointyTop,
        FlatTop,
    }

    public static class Hexes {
        const float SQRT_3 = 1.73205081f;

        public static GridTypes GridType(this OffsetSystems s) => s switch {
            OffsetSystems.OddR => GridTypes.PointyTop,
            OffsetSystems.EvenR => GridTypes.PointyTop,
            OffsetSystems.OddQ => GridTypes.FlatTop,
            OffsetSystems.EvenQ => GridTypes.FlatTop,
            _ => throw new System.InvalidOperationException("Invalid offset system")
        };

        public static Hex Neighbour(this Hex c, Hex offset) {
            return c + offset;
        }
        public static Hex RotateAroundZero(this Hex c, int hexRotations = 1) { 
            hexRotations %= 6;
            if (hexRotations < 0) hexRotations += 6;
            for (var i = 0; i < hexRotations; i++) c = RotateAroundZeroOnceClockwise(c);
            return c;
        }

        private static Hex RotateAroundZeroOnceCounterclockwise(Hex c) => (-c.r, -c.S);
        private static Hex RotateAroundZeroOnceClockwise(Hex c) => (-c.S, -c.q);

        public static Hex Round(float q, float r) {
            var intQ = Mathf.RoundToInt(q);
            var intR = Mathf.RoundToInt(r);
            var s = -q-r;
            var intS = Mathf.RoundToInt(s);

            var qFrac = Mathf.Abs(intQ - q);
            var rFrac = Mathf.Abs(intR - r);
            var sFrac = Mathf.Abs(intS - s);

            if (qFrac > rFrac && qFrac > sFrac) {
                intQ = -intR - intS;
            } else if (rFrac > sFrac) {
                intR = -intQ - intS;
            } else {
                intS = -intQ - intR;
            }
            return new Hex(intQ, intR);
        }

        public static Hex PixelToHex(float x, float y, GridTypes g, float d) {
            return g switch {
                GridTypes.FlatTop => Round( 
                    2f / 3f * x / d,
                    -1f / 3f * x / d  + SQRT_3 / 3f * y / d
                ),
                GridTypes.PointyTop => Round(
                    SQRT_3 / 3f * x / d - 1f * y / d,
                    2f / 3f * y / d
                ),
                _ => throw new Exception("Invalid grid type")
            };
        }

        /// <param name="d">the "size" of the hex's side. The hex is 2d wide at its longest.</param>
        public static (float x, float y) HexToPixel(this Hex hex, GridTypes g, float d) {
            return g switch { 
                GridTypes.PointyTop => ( d * (SQRT_3 * hex.q + SQRT_3 / 2 * hex.r) , d * (3f / 2 * hex.r) ),
                GridTypes.FlatTop   => ( d * (3f / 2 * hex.q) , d * (SQRT_3 / 2 * hex.q + SQRT_3 * hex.r) ),
                _ => throw new Exception("Invalid grid type")
            };
        }
    }
}