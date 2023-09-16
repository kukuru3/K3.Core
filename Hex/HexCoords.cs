using System;
using System.Collections.Generic;
using UnityEngine;

namespace K3.Hex {
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

        public void Deconstruct(out int Q, out int R) { Q = q; R = r; }
        public void Deconstruct(out int Q, out int R, out int S) { Q = q; R = r; S = this.S; }

        public static implicit operator Hex((int q, int r) t) => new Hex(t.q, t.r);

        public static int Distance(Hex a, Hex b) {
            var delta = a - b;
            return Math.Abs(delta.q) + Math.Abs(delta.r) + Math.Abs(delta.S) / 2;
        }

        public int DistanceTo(Hex b) => Distance(this, b);
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
        public static void RotateClockwise(this Hex c, int howManyHexRotations = 1) { throw new System.NotImplementedException(); }   

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

        public static readonly Hex[] Neighbours = new[] { OffsetsP.East, OffsetsP.SouthEast, OffsetsP.SouthWest, OffsetsP.West, OffsetsP.NorthWest, OffsetsP.NorthEast };

        public static (int x, int y) ToOffsetCoordinates(Hex hex, OffsetSystems o) {
            return o switch {
                OffsetSystems.OddR => ToOddR(hex),
                OffsetSystems.EvenR => ToEvenR(hex),
                OffsetSystems.OddQ => ToOddQ(hex),
                OffsetSystems.EvenQ => ToEvenQ(hex),
                _ => throw new System.InvalidOperationException("Invalid offset system")
            };
        }

        public static Hex ToHexCoordinates(int row, int column, OffsetSystems o) {
            return o switch { 
                OffsetSystems.OddR => FromOddR(row, column),
                OffsetSystems.EvenR => FromEvenR(row, column),
                OffsetSystems.OddQ => FromOddQ(row, column),
                OffsetSystems.EvenQ => throw new System.NotImplementedException(),
                _ => throw new System.InvalidOperationException("Invalid offset system")
            };
        }

        public static IEnumerable<Hex> InRadius(Hex center, int N) {
            for (var q = -N; q <= N; q++) {
                for (var r = Math.Max(-N, -q - N); r <= Math.Min(N, -q + N); r++) {
                    yield return center + (q, r);
                }
            }
        }

        private static (int x, int y) ToOddR(Hex hex) {
            var x = hex.q + (hex.r - (hex.r & 1)) / 2;
            var y = hex.r;
            return (x, y);
        }
        
        private static (int x, int y) ToEvenR(Hex hex) {
            var x = hex.q + (hex.r + (hex.r & 1)) / 2;
            var y = hex.r;
            return (x, y);
        }

        private static Hex FromOddR(int row, int col) {
            var q = col + (row - (row & 1)) / 2;
            var r = row;
            return new Hex(q, r);
        }

        private static Hex FromEvenR(int row, int col) {
            var q = col + (row + (row & 1)) / 2;
            var r = row;
            return new Hex(q, r);
        }

        private static (int x, int y) ToOddQ(Hex hex) {
            var x = hex.q;
            var y = hex.r + (hex.q - (hex.q & 1)) / 2;
            return (x, y);
        }

        private static (int x, int y) ToEvenQ(Hex hex) {
            var x = hex.q;
            var y = hex.r + (hex.q + (hex.q & 1)) / 2;
            return (x, y);
        }
        private static Hex FromOddQ(int row, int col) {
            var q = col;
            var r = row - (col - (col & 1)) / 2;
            return new Hex(q, r);
        }
    }

    public struct OffsetsP {
        public static Hex East => (1,0);
        public static Hex West => (-1,0);
        public static Hex NorthEast => (+1,-1);
        public static Hex NorthWest => (0,-1);
        public static Hex SouthEast => (0,+1);
        public static Hex SouthWest => (-1,+1);
    }

    public struct OffsetsF {
        public static Hex SouthEast => (1,0);
        public static Hex NorthEast => (1,-1);
        public static Hex South => (0,1);
        public static Hex North => (0,-1);
        public static Hex SouthWest => (-1,1);
        public static Hex NorthWest => (-1,0);
    }
}