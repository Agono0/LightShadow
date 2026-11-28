using UnityEngine;

namespace LightShadow.Player
{
    // Pure maths for fitting a delivered player/torch model to the first-person
    // rig. Delivered files carry small named markers ("Grip", "Muzzle") inside
    // the mesh; the editor builder reads their positions and uses these helpers
    // so the same fit rules are testable without a Unity scene.
    public static class PlayerRigFit
    {
        public const string GripName = "Grip";
        public const string MuzzleName = "Muzzle";

        // Where the model root must sit so its Grip marker lands on the hand's
        // grip point. modelRotation and modelScale are what the model carries
        // under the hand; gripLocal is the marker position in model space.
        public static Vector3 RootOffsetForGrip(Vector3 handGripPoint, Vector3 gripLocal,
            Quaternion modelRotation, float modelScale)
        {
            return handGripPoint - modelRotation * (gripLocal * modelScale);
        }

        // Muzzle marker position in the hand's space for a model whose root
        // sits at rootOffset. Used to start the beam at the lens.
        public static Vector3 MuzzleInHandSpace(Vector3 rootOffset, Vector3 muzzleLocal,
            Quaternion modelRotation, float modelScale)
        {
            return rootOffset + modelRotation * (muzzleLocal * modelScale);
        }

        // A marker is usable only with finite coordinates; a model that
        // exports a broken group must fall back, not teleport the torch.
        public static bool IsUsable(Vector3 point)
        {
            return IsFinite(point.x) && IsFinite(point.y) && IsFinite(point.z)
                && point.sqrMagnitude < 25f; // markers sit within 5 m of the origin
        }

        static bool IsFinite(float v)
        {
            return !float.IsNaN(v) && !float.IsInfinity(v);
        }

        // Capsule for the player body, independent of any model: the delivered
        // body is visual only and never changes the physics shape.
        public static void ConfigureBody(GameObject root, float height, float radius)
        {
            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null) body = root.AddComponent<Rigidbody>();
            // The CharacterController owns movement. A kinematic body with
            // frozen rotation gives trigger volumes and physics queries a
            // stable owner without a second solver fighting the controller.
            body.isKinematic = true;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.mass = 70f;
        }
    }
}
