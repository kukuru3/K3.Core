using System;
using UnityEngine;

namespace K3.Hex {
    public struct HexCoords {
        public readonly int q;
        public readonly int r;
        public readonly int S => -q - r;

        public HexCoords(int q, int r) {
            this.q = q;
            this.r = r;
        }

        public static HexCoords operator +(HexCoords a, HexCoords b) => new HexCoords(a.q + b.q, a.r + b.r);
        public static HexCoords operator -(HexCoords a, HexCoords b) => new HexCoords(a.q - b.q, a.r - b.r);

        public void Deconstruct(out int Q, out int R) { Q = q; R = r; }
        public void Deconstruct(out int Q, out int R, out int S) { Q = q; R = r; S = this.S; }

        public static implicit operator HexCoords((int q, int r) t) => new HexCoords(t.q, t.r);

        public static int Distance(HexCoords a, HexCoords b) {
            var delta = a - b;
            return Math.Abs(delta.q) + Math.Abs(delta.r) + Math.Abs(delta.S) / 2;
        }

        public int DistanceTo(HexCoords b) => Distance(this, b);
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

        public static HexCoords Neighbour(this HexCoords c, HexCoords offset) {
            return c + offset;
        }
        public static void RotateClockwise(this HexCoords c, int howManyHexRotations = 1) { throw new System.NotImplementedException(); }   

        public static HexCoords Round(float q, float r) {
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
            return new HexCoords(intQ, intR);
        }

        public static HexCoords PixelToHex(float x, float y, GridTypes g, float d) {
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

        public static (float x, float y) HexToPixel(this HexCoords hex, GridTypes g, float d) {
            return g switch { 
                GridTypes.PointyTop => ( d * (SQRT_3 * hex.q + SQRT_3 / 2 * hex.r) , d * (3f / 2 * hex.r) ),
                GridTypes.FlatTop   => ( d * (3f / 2 * hex.q) , d * (SQRT_3 / 2 * hex.q + SQRT_3 * hex.r) ),
                _ => throw new Exception("Invalid grid type")
            };
        }

        public static (int x, int y) ToOffsetCoordinates(HexCoords hex, OffsetSystems o) {
            return o switch {
                OffsetSystems.OddR => ToOddR(hex),
                OffsetSystems.EvenR => ToEvenR(hex),
                OffsetSystems.OddQ => ToOddQ(hex),
                OffsetSystems.EvenQ => ToEvenQ(hex),
                _ => throw new System.InvalidOperationException("Invalid offset system")
            };
        }

        public static HexCoords ToHexCoordinates(int row, int column, OffsetSystems o) {
            return o switch { 
                OffsetSystems.OddR => FromOddR(row, column),
                OffsetSystems.EvenR => FromEvenR(row, column),
                OffsetSystems.OddQ => FromOddQ(row, column),
                OffsetSystems.EvenQ => throw new System.NotImplementedException(),
                _ => throw new System.InvalidOperationException("Invalid offset system")
            };
        }

        private static (int x, int y) ToOddR(HexCoords hex) {
            var x = hex.q + (hex.r - (hex.r & 1)) / 2;
            var y = hex.r;
            return (x, y);
        }
        
        private static (int x, int y) ToEvenR(HexCoords hex) {
            var x = hex.q + (hex.r + (hex.r & 1)) / 2;
            var y = hex.r;
            return (x, y);
        }

        private static HexCoords FromOddR(int row, int col) {
            var q = col + (row - (row & 1)) / 2;
            var r = row;
            return new HexCoords(q, r);
        }

        private static HexCoords FromEvenR(int row, int col) {
            var q = col + (row + (row & 1)) / 2;
            var r = row;
            return new HexCoords(q, r);
        }

        private static (int x, int y) ToOddQ(HexCoords hex) {
            var x = hex.q;
            var y = hex.r + (hex.q - (hex.q & 1)) / 2;
            return (x, y);
        }

        private static (int x, int y) ToEvenQ(HexCoords hex) {
            var x = hex.q;
            var y = hex.r + (hex.q + (hex.q & 1)) / 2;
            return (x, y);
        }
        private static HexCoords FromOddQ(int row, int col) {
            var q = col;
            var r = row - (col - (col & 1)) / 2;
            return new HexCoords(q, r);
        }
    }

    public struct OffsetsP {
        public static HexCoords East => (1,0);
        public static HexCoords West => (-1,0);
        public static HexCoords NorthEast => (+1,-1);
        public static HexCoords NorthWest => (0,-1);
        public static HexCoords SouthEast => (0,+1);
        public static HexCoords SouthWest => (-1,+1);
    }

    public struct OffsetsF {
        public static HexCoords SouthEast => (1,0);
        public static HexCoords NorthEast => (1,-1);
        public static HexCoords South => (0,1);
        public static HexCoords North => (0,-1);
        public static HexCoords SouthWest => (-1,1);
        public static HexCoords NorthWest => (-1,0);
    }
}