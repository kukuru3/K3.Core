using UnityEngine;

namespace K3 {

    public static class PoseUtility {
        /// <returns>pose necessary to go from "from" to "to"</returns>
        public static Pose GetRelativePose(this Pose from, Pose to) {
            Quaternion inverseLhsRotation = Quaternion.Inverse(from.rotation);
            Pose relativePose = new Pose {
                position = inverseLhsRotation * (to.position - from.position),
                rotation = inverseLhsRotation * to.rotation
            };
            return relativePose;
        }
        // wait, isn't this precisely the same as GetRelativePose()?
        public static Pose GetDeltaAToB(Pose a, Pose b) {
            return Mul(a.Inverse(), b);
        }
        public static Pose Inverse(this Pose p) {
            var invRot = Quaternion.Inverse(p.rotation);
            return new Pose(invRot * -p.position, invRot);
        }

        public static Pose Mul(this Pose parent, Pose child) {
            return child.GetTransformedBy(parent);
        }

        public static Pose WorldPose(this Transform transform) {
            return new Pose(transform.position, transform.rotation);
        }

        public static Pose LocalPose(this Transform transform) {
            return new Pose(transform.localPosition, transform.localRotation);
        }

        public static void AssumePose(this Transform transform, Pose pose) {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        public static void AssumeLocalPose(this Transform transform, Pose pose) {
            transform.SetLocalPositionAndRotation(pose.position, pose.rotation);
        }

        public static bool Identical(Pose a, Pose b, float distanceEpsilon = 0.01f, float angleEpsilon = 1f) {
            var diff = GetRelativePose(a, b);
            return (diff.position.magnitude < distanceEpsilon) && (Quaternion.Angle(Quaternion.identity, diff.rotation) < angleEpsilon);
        }

        public static Vector3 TransformPoint(this Pose worldspacePose, Vector3 localPoint) => worldspacePose.position + worldspacePose.rotation * localPoint;

        public static Vector3 TransformDirection(this Pose worldspacePose, Vector3 direction) => worldspacePose.rotation * direction;
        public static Vector3 InverseTransformDirection(this Pose worldspacePose, Vector3 direction) => Quaternion.Inverse(worldspacePose.rotation) * direction;

        public static Ray TransformRay(this Pose deltaPose, Ray ray) {
            return new(deltaPose.TransformPoint(ray.origin), deltaPose.TransformDirection(ray.direction));
        }
    }

    public static class TransformUtility {
        //public static Pose WorldPose(this Transform t) {
        //    return new Pose(t.position, t.rotation);
        //}
        /// <param name="P">parent</param>
        /// <param name="C">child</param>
        /// <param name="R">reference</param>
        /// <param name="scale">final scale</param>
        public static void MoveAndRotateParentSoThatChildCoincidesWithReferenceTransform(Transform P, Transform C, Transform R) {
            const float scale = 1f;
            if (P == null || C == null || R == null) {
                Debug.LogWarning($"Unable to coincide parent, all references must be non-null) (P,C,R = {P}, {C}, {R})");
                return;
            }
            var rotationParentToChild = Quaternion.Inverse(P.rotation) * C.rotation; // normally this is child local rotation, but we can't guarantee it's an immediate child
            var childRelativePos = P.InverseTransformPoint(C.position);
            var parentRot = R.rotation * Quaternion.Inverse(rotationParentToChild);

            P.SetPositionAndRotation(
                R.position - parentRot * childRelativePos * scale,
                parentRot
            );

            P.localScale = Vector3.one * scale;
        }

        public static void MoveAndRotateParentSoThatChildCoincidesWithReferenceValues(Transform P, Transform C, Quaternion referenceRotation, Vector3 referencePosition) {
            const float scale = 1f;
            if (P == null || C == null) {
                Debug.LogWarning($"Unable to coincide parent, all references must be non-null) (P,C = {P}, {C})");
                return;
            }
            var rotationParentToChild = Quaternion.Inverse(P.rotation) * C.rotation; // normally this is child local rotation, but we can't guarantee it's an immediate child
            var childRelativePos = P.InverseTransformPoint(C.position);
            var parentRot = referenceRotation * Quaternion.Inverse(rotationParentToChild);

            P.SetPositionAndRotation(
                referencePosition - parentRot * childRelativePos * scale,
                parentRot
            );

            P.localScale = Vector3.one * scale;
        }



        public static Pose GetPoseSuchThatChildCoincidesWithReferenceValues(Transform P, Transform C, Quaternion referenceRotation, Vector3 referencePosition) {
            const float scale = 1f;
            if (P == null || C == null) {
                Debug.LogWarning($"Unable to coincide parent, all references must be non-null) (P,C = {P}, {C})");
                return default;
            }
            var rotationParentToChild = Quaternion.Inverse(P.rotation) * C.rotation; // normally this is child local rotation, but we can't guarantee it's an immediate child
            var childRelativePos = P.InverseTransformPoint(C.position);
            var parentRot = referenceRotation * Quaternion.Inverse(rotationParentToChild);

            return new Pose(
                referencePosition - parentRot * childRelativePos * scale,
                parentRot
            );
            
        }

        public static Quaternion MoveTowards(Quaternion a, Quaternion b, float deltaAngle) {
            var angle = Quaternion.Angle(a,b);
            if (angle < 0.001f) return b;
            var t = deltaAngle / angle;
            return Quaternion.Slerp(a, b, t);
        }

        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
            if (Time.deltaTime < Mathf.Epsilon) return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }

        public static void SetLocalX(this Transform t, float x) { var p = t.localPosition; p.x = x; t.localPosition = p; }
        public static void SetLocalY(this Transform t, float y) { var p = t.localPosition; p.y = y; t.localPosition = p; }
        public static void SetLocalZ(this Transform t, float z) { var p = t.localPosition; p.z = z; t.localPosition = p; }

    }


}