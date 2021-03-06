using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace K3 {
    static public class Geometry {

        /// <summary> http://mathworld.wolfram.com/Ellipse-LineIntersection.html </summary>
        static public Vector2 GetForwardIntersectOnEllipse(Vector2 direction, Vector2 ellipseAxes) {
            var a = ellipseAxes.x; var b = ellipseAxes.y;
            var x = direction.x; var y = direction.y;
            var coefficient = (a * b) / Mathf.Sqrt(a * a * y * y + b * b * x * x);
            return direction * coefficient;
        }

        static public float ProjectPointOnSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB) {
            float l2 = (segmentA - segmentB).sqrMagnitude;
            if (l2 < 0.000001f) return 0f;
            float t = Vector2.Dot(point - segmentA, segmentB - segmentA) / l2;
            return t;
        }

        static public float Angularity(Vector2 g, Vector2 h) {
            g.Normalize(); h.Normalize();
            return (g.x * h.y - g.y * h.x);
        }

        static public float ProjectPointOnSegment(Vector3 p, Vector3 a, Vector3 b) {
            var axis = (b - a).normalized;
            p = Vector3.Project(p, axis);
            return Vector3.Dot(p - a, axis) / Vector3.Distance(a, b);
        }

        public static Vector2 Rotated(this Vector2 v, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }


        static public (Vector3 pointOnA, Vector3 pointOnB) ClosestPointsBetweenTwoLineSegments(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            static Vector3 ConstrainToSegment(Vector3 position, Vector3 segA, Vector3 segB) {
                var ba = segB - segA; var t = Vector3.Dot(position - segA, ba) / Vector3.Dot(ba, ba);
                return Vector3.Lerp(segA, segB, t);
            }

            var dc = d-c; var lineDirSqrMag = Vector3.Dot(dc, dc);
            var inPlaneA = a-((Vector3.Dot(a-c, dc)/lineDirSqrMag)*dc);
            var inPlaneB = b-((Vector3.Dot(b-c, dc)/lineDirSqrMag)*dc);
            var inPlaneBA = inPlaneB-inPlaneA;
            var t = Vector3.Dot(c-inPlaneA, inPlaneBA)/ Vector3.Dot(inPlaneBA, inPlaneBA);
            t = (inPlaneA != inPlaneB) ? t : 0f; // Zero's t if parallel
            var segABtoLineCD = Vector3.Lerp(a, b, t);

            var segCDtoSegAB = ConstrainToSegment(segABtoLineCD, c, d);
            var segABtoSegCD = ConstrainToSegment(segCDtoSegAB, a, b);

            return (segABtoSegCD, segCDtoSegAB);
        }

        public static Vector3? NormalizedInterceptVector(Vector3 interceptorPosition, Vector3 targetPosition, Vector3 targetVelocity, float interceptorVelocityMagnitude) {
            var O = interceptorPosition;
            var P = targetPosition;
            var u = targetVelocity;
            var k = interceptorVelocityMagnitude;

            var termA = u.sqrMagnitude - k * k;
            var termB = 2 * Vector3.Dot(P - O, u);
            var termC = (P - O).sqrMagnitude;

            var t = Numbers.QuadraticEquation(termA, termB, termC).SmallestPositive();
            if (t.HasValue) {
                var expectedTargetPosition = P + u * t.Value;
                return (expectedTargetPosition - interceptorPosition).normalized;
            } else {
                return (targetPosition - interceptorPosition).normalized;
            }
        }

        public static Vector3? GetBestFittingPlaneNormal(IEnumerable<Vector3> points) {
            var n = points.Count();
            if (n < 3) return null;
            var centroid = Vector3.zero;
            foreach (var point in points) centroid += point;
            centroid /= n;

            // calculate full 3x3 covariance matrix, excluding symmetries
            float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;

            foreach (var point in points) {
                var r = point - centroid;
                xx += r.x * r.x; xy += r.x * r.y; xz += r.x * r.z;
                yy += r.y * r.y; yz += r.y * r.z; zz += r.z * r.z;
            }

            var detX = yy * zz - yz * yz;
            var detY = xx * zz - xz * xz;
            var detZ = xx * yy - xy * xy;

            var detMax = Mathf.Max(detX, detY, detZ);

            if (detMax < 0f) return null;

            // pick path with best conditioning
            if (Mathf.Approximately(detMax, detX))
                return new Vector3(detX, xz * yz - xy * zz, xy * yz - xz * yy).normalized;
            else if (Mathf.Approximately(detMax, detY))
                return new Vector3(xz * yz - xy * zz, detY, xy * xz - yz * xx).normalized;
            else if (Mathf.Approximately(detMax, detZ))
                return new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, detZ).normalized;

            return null;
        }

        public static Vector2 Flatten(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector3 Deflatten(this Vector2 vector) => new Vector3(vector.x, 0, vector.y);


    }
}
