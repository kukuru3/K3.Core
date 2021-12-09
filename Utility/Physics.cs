using UnityEngine;

namespace K3 {
    static public class PhysicsUtils {
        static Collider[] _overlapResults = new Collider[1];
        static Vector3[] _capsuleDirections = new[] { Vector3.right, Vector3.up, Vector3.forward };

        static public bool ColliderOverlapsGeometry(Collider c, int layerMask) {
            if (c == null) return false;
            var t = c.transform;
            if (c is BoxCollider bc) {
                var worldCenter = t.TransformPoint(bc.center);
                var halfExtents = (bc.size * 0.5f);
                halfExtents.Scale(t.lossyScale);
                return Physics.OverlapBoxNonAlloc(worldCenter, halfExtents, _overlapResults, t.rotation, layerMask) > 0;
            } if (c is CapsuleCollider cc) {
                var d = _capsuleDirections[cc.direction] * 0.5f;
                var p0 = t.TransformPoint(cc.center + d);
                var p1 = t.TransformPoint(cc.center - d);
                var s = t.lossyScale;
                var leastScale = Mathf.Max(s.x, s.y, s.z);

                return Physics.OverlapCapsuleNonAlloc(p0, p1, cc.radius * leastScale, _overlapResults, layerMask) > 0;
            }
            throw new System.NotImplementedException($"Static overlap check of collider of type {c.GetType()} is not yet supported - go pester KUKURU3 about it");
        }

        static public (Vector3 a, Vector3 b) GetCapsuleColliderWorldspaceSegment(this CapsuleCollider cc) {
            var t = cc.transform;
            var d = _capsuleDirections[cc.direction] * 0.5f;
            var p0 = t.TransformPoint(cc.center + d);
            var p1 = t.TransformPoint(cc.center - d);
            return (p0, p1);
        }

        static public Vector3 AverageContactNormal(this Collision collision) {
            var averageContactNormal = Vector3.zero;
            int counter = 0;
            foreach (var contact in collision.contacts) { averageContactNormal += contact.normal; counter++; }
            return averageContactNormal / counter;
        }
    }
}