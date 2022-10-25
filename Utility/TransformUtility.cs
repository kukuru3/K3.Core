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
    }


}