using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tools
{
    // The unified ending cinematic rig for 03_Ending (spec:
    // Docs/ENDING_VO_SPEC.md): nine camera-rail shots over the existing dawn
    // set, a placeholder third-person Nour (bro's girl model replaces it: the
    // controller moves whatever sits in its subject field), a wraith silhouette
    // third-floor window for shot 6, and the subtitle line on the ending
    // canvas. Waypoints are plain transforms; the controller eases between
    // them, so timing and framing can be tuned by moving transforms, not code.
    public static partial class SceneBuilder
    {
        static void BuildEndingCinematic(Transform setRoot, Palette palette, Camera cam, EndingController ending)
        {
            var rig = Group("EndingCinematic", setRoot);
            Transform r = rig.transform;

            // ---- placeholder Nour --------------------------------------------
            // Simple readable figure: coat capsule + head. The girl model and
            // its animations (idle / walk / turn / crank / kneel) come from
            // bro's side; the controller's subject reference is the swap point.
            var nour = Group("EndingNour_Placeholder", r);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(nour.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            body.transform.localScale = new Vector3(0.42f, 0.85f, 0.32f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = palette.wall;
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(nour.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = palette.stone;
            nour.transform.position = new Vector3(0.5f, 0f, 5.2f);
            nour.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // facing the gate (-z)

            // ---- wraith silhouette (shot 6) ----------------------------------
            // A flat dark figure against a third-floor window; bro's wraith
            // model replaces it. Windows row: x = -18 + i*4.5, y 5.1, z 17.92.
            Box(r, "WraithSilhouette_Placeholder", new Vector3(-4.5f, 4.95f, 17.86f),
                new Vector3(0.85f, 1.75f, 0.05f), palette.ceiling, false);

            // ---- camera + subject waypoints ----------------------------------
            // Positions against the dawn set: gate at z=4, school at z=20,
            // pavement at z=-6, sky panel at z=40, sun at (-21, 1.6, 28).
            EndingCinematicController cine = ending.gameObject.AddComponent<EndingCinematicController>();
            cine.subject = nour.transform;
            cine.shots = new[]
            {
                Shot(r, "S1_crank", 7f,
                    Waypoint(r, "S1_camA", new Vector3(1.3f, 1.5f, 6.2f), new Vector3(0f, 1.3f, 4.4f)),
                    Waypoint(r, "S1_camB", new Vector3(1.0f, 1.45f, 5.6f), new Vector3(0f, 1.3f, 4.3f)),
                    Waypoint(r, "S1_subjA", new Vector3(0.5f, 0f, 5.2f), new Vector3(0.5f, 1f, 0f)),
                    null,
                    "SN_Ending_01"),
                Shot(r, "S2_reveal", 11f,
                    Waypoint(r, "S2_camA", new Vector3(0f, 0.6f, 7.5f), new Vector3(0f, 1.5f, 3.5f)),
                    Waypoint(r, "S2_camB", new Vector3(0f, 0.9f, 6.5f), new Vector3(0f, 1.4f, 3.0f)),
                    Waypoint(r, "S2_subjA", new Vector3(0.5f, 0f, 5.2f), new Vector3(0.5f, 1f, 0f)),
                    Waypoint(r, "S2_subjB", new Vector3(0f, 0f, 2.2f), new Vector3(0f, 1f, -6f)),
                    "SN_Ending_02", "SN_Ending_03"),
                Shot(r, "S3_crane", 8f,
                    Waypoint(r, "S3_camA", new Vector3(0f, 2.2f, 5.5f), new Vector3(0f, 1.5f, 20f)),
                    Waypoint(r, "S3_camB", new Vector3(0f, 7.5f, 1.5f), new Vector3(0f, 3.5f, 20f)),
                    null, null,
                    "SN_Ending_04"),
                Shot(r, "S4_box", 7f,
                    Waypoint(r, "S4_camA", new Vector3(0.7f, 0.9f, 0.8f), new Vector3(0f, 0.45f, 1.4f)),
                    Waypoint(r, "S4_camB", new Vector3(0.5f, 0.75f, 0.2f), new Vector3(0f, 0.4f, 1.4f)),
                    Waypoint(r, "S4_subjA", new Vector3(0f, -0.35f, 1.6f), new Vector3(0f, 0.5f, -2f)),
                    null,
                    "SN_Ending_05"),
                Shot(r, "S5_face", 11f,
                    Waypoint(r, "S5_camA", new Vector3(0.25f, 1.5f, 0.4f), new Vector3(0f, 1.5f, 1.8f)),
                    Waypoint(r, "S5_camB", new Vector3(0.15f, 1.5f, 0.0f), new Vector3(0f, 1.5f, 1.8f)),
                    Waypoint(r, "S5_subjA", new Vector3(0f, 0f, 1.8f), new Vector3(0f, 1f, 4f)),
                    null,
                    "SN_Ending_06", "SN_Ending_07"),
                Shot(r, "S6_window", 5f,
                    Waypoint(r, "S6_camA", new Vector3(1.5f, 1.6f, -2.0f), new Vector3(-4.5f, 5.0f, 17.9f)),
                    Waypoint(r, "S6_camB", new Vector3(1.2f, 1.8f, -2.4f), new Vector3(-4.5f, 5.0f, 17.9f)),
                    null, null,
                    "SN_Ending_08"),
                Shot(r, "S7_walkaway", 11f,
                    Waypoint(r, "S7_camA", new Vector3(0f, 1.7f, -1.5f), new Vector3(0f, 1.2f, -8f)),
                    Waypoint(r, "S7_camB", new Vector3(0f, 1.8f, -6.5f), new Vector3(0f, 1.2f, -12f)),
                    Waypoint(r, "S7_subjA", new Vector3(0f, 0f, 0.5f), new Vector3(0f, 1f, -10f)),
                    Waypoint(r, "S7_subjB", new Vector3(0f, 0f, -6.0f), new Vector3(0f, 1f, -16f)),
                    "SN_Ending_09", "SN_Ending_10", "SN_Ending_11"),
                Shot(r, "S8_facade", 6f,
                    Waypoint(r, "S8_camA", new Vector3(0f, 2.0f, -10.5f), new Vector3(0f, 3.5f, 20f)),
                    Waypoint(r, "S8_camB", new Vector3(0f, 2.4f, -12.5f), new Vector3(0f, 3.8f, 20f)),
                    null, null,
                    "SN_Ending_12", "SN_Ending_13"),
                Shot(r, "S9_title", 4f,
                    Waypoint(r, "S9_camA", new Vector3(0f, 3.0f, -8.0f), new Vector3(-8f, 12f, 38f)),
                    Waypoint(r, "S9_camB", new Vector3(0f, 3.4f, -8.6f), new Vector3(-10f, 13f, 38f)),
                    null, null,
                    "SN_Ending_14"),
            };

            // ---- subtitle line on the ending canvas ---------------------------
            Canvas canvas = ending.GetComponent<Canvas>();
            Transform safe = canvas != null ? canvas.transform.Find("SafeArea") : null;
            if (safe != null)
            {
                Text subtitle = MakeText(safe, "CinematicSubtitle", "", 30,
                    new Color(0.92f, 0.89f, 0.82f), TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 90f), new Vector2(1400f, 120f));
                ending.cinematicSubtitle = subtitle;
            }
        }

        static Transform Waypoint(Transform parent, string name, Vector3 pos, Vector3 lookAt)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Vector3 dir = lookAt - pos;
            go.transform.rotation = dir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(dir.normalized, Vector3.up)
                : Quaternion.identity;
            return go.transform;
        }

        static EndingCinematicController.Shot Shot(Transform parent, string id, float seconds,
            Transform camA, Transform camB, Transform subjA, Transform subjB, params string[] lineIds)
        {
            camA.name = id + "_camA";
            camB.name = id + "_camB";
            return new EndingCinematicController.Shot
            {
                id = id,
                seconds = seconds,
                from = camA,
                to = camB,
                subjectFrom = subjA,
                subjectTo = subjB,
                lineIds = lineIds,
            };
        }
    }
}
