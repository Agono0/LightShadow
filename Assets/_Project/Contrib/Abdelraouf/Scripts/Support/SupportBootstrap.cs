using LightShadow.Contrib.Abdelraouf.Captions;
using LightShadow.Contrib.Abdelraouf.Hints;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Support
{
    // Adds the hint and caption services next to each other under one object.
    // The H2 level contribution puts this component in the school scene and
    // assigns the two data files.
    //
    // Debug test rig (editor and development builds only, OFF by default):
    // compiled out of release builds entirely. In an editor or development
    // build it stays inert unless the PlayerPrefs int
    // "abdelraouf.support.testrig" is set to 1 on purpose. When on:
    //   F9  raises the next simulated cue from a rotating direction, with a
    //       "[SIM]" prefix on the caption
    //   F10 shows the next level of a built-in stub hint (no count, no save)
    // The rig only calls the public caption API and a stub hint. It never
    // touches puzzles, saves, enemies or audio. Simulated cues say nothing
    // about whether real game sounds have captions.
    public class SupportBootstrap : MonoBehaviour
    {
        public TextAsset hintsJson;
        public TextAsset captionsJson;

        public const string TestRigPrefsKey = "abdelraouf.support.testrig";

        void Awake()
        {
            var hints = gameObject.AddComponent<HintService>();
            hints.hintsJson = hintsJson;
            var captions = gameObject.AddComponent<CaptionService>();
            captions.captionsJson = captionsJson;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static readonly string[] RigKeys =
        {
            CaptionKeys.FootstepsNear,
            CaptionKeys.Whisper,
            CaptionKeys.DoorSlam,
            CaptionKeys.PowerHum,
            CaptionKeys.ShadowGrowl,
        };

        int rigCue;

        public static bool TestRigEnabled => PlayerPrefs.GetInt(TestRigPrefsKey, 0) == 1;

        void Update()
        {
            if (!TestRigEnabled) return;

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                // Cycle left, right, behind, front.
                Vector3[] directions = { -cam.transform.right, cam.transform.right, -cam.transform.forward, cam.transform.forward };
                Vector3 position = cam.transform.position + directions[rigCue % directions.Length] * 6f;
                CaptionService.RaiseInternal(RigKeys[rigCue % RigKeys.Length], position, true);
                rigCue++;
            }

            if (Input.GetKeyDown(KeyCode.F10) && HintService.Instance != null)
            {
                HintService.Instance.ShowStubHint();
            }
        }
#endif
    }
}
