using UnityEngine;

namespace K3 {
    public static class RotationUtility {
        // Warning: Quaternion black magic fuckery ahead:
        public static (float angle, Vector3 axis) GetRelativeRotation(this Quaternion from, Quaternion to) {

            // normally a relative rotation is achieved by doing Quaternion.Inverse(from) * to
            // however, in practice, if working with worldspace rotations with parent eulers greater than 360, it can get wonky for some reason.

            var invrot = Quaternion.Inverse(from);
            // if (invrot.w * to.w < 0f) invrot = new Quaternion(-invrot.x, -invrot.y, -invrot.z, -invrot.w);

            var epsilon = invrot * to;
            if (epsilon.IsIdentity()) return (0f, Vector3.forward);

            epsilon.normalized.ToAngleAxis(out var angle, out var axis);

            // angles greater than 180° are possible for some reason. Likely due to that flip we had to do earlier?
            if (angle > 180f) return (360f - angle, -axis);
            else return (angle, axis);

            // Consult https://stackoverflow.com/questions/42428136/quaternion-is-flipping-sign-for-very-similar-rotations 
            // which seems to describe and solve the same problem, maybe in a general, less hacky way.
        }

        /// <summary>Returns a NORMALIZED rotation with identical axis to input axis, but whose angle 
        /// does not exceed maxAngle in either direction </summary>
        static public Quaternion LimitAngle(this Quaternion q, float maxAngle) {
            if (q.IsIdentity() || maxAngle < float.Epsilon) return Quaternion.identity;
            q.ToAngleAxis(out var angle, out var axis);
            if (angle > maxAngle) return Quaternion.AngleAxis(maxAngle, axis).normalized;
            else return q.normalized;
        }

        static public bool IsIdentity(this Quaternion q) => Mathf.Approximately(Mathf.Abs(q.w), 1f);

        static public bool IsNormalized(this Quaternion q) => Mathf.Approximately(Magnitude(q), 1f);

        static public float Magnitude(this Quaternion q) => Quaternion.Dot(q, q);


        /// <summary>Calculates corrections needed to ideally stop at a desired target rotation, for a rotational system 
        /// that has angular acceleration and maximum angular speed limitations. </summary> 
        /// <returns>the new angular velocity</returns>
        public static Quaternion UpdateAngularVelocityForTargetedRotationSystem(
            Quaternion currentRotation,
            Quaternion targetRotation,
            Quaternion currentAngularVelocity,
            float acceleration,
            float timespan,
            float leeway = 0.1f,
            float maxAngularSpeed = float.MaxValue
        ) {
            var epsilon = GetRelativeRotation(currentRotation, targetRotation);
            var desiredOmegaMagnitude = Mathf.Sqrt(2 * acceleration * epsilon.angle) * (1f - leeway);
            if (desiredOmegaMagnitude > 180f) desiredOmegaMagnitude = 180f;
            var desiredOmega = Quaternion.AngleAxis(desiredOmegaMagnitude, epsilon.axis);

            var correction = Quaternion.Inverse(currentAngularVelocity) * desiredOmega;
            correction = correction.LimitAngle(acceleration * timespan);

            var omega = currentAngularVelocity * correction;
            omega = omega.normalized.LimitAngle(maxAngularSpeed);
            return omega;
        }
    }
}
