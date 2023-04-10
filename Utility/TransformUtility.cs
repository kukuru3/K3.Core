using UnityEngine;

namespace K3 {

    public static class TransformUtility {
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
    }


}