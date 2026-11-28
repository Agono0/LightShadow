using System.Collections;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // The single unified ending: a third-person cinematic played at the top of
    // 03_Ending, before the dawn screen and the study recap (spec:
    // Docs/ENDING_VO_SPEC.md). Every run gets the same 14-line, 9-shot
    // sequence; the recap afterwards still personalizes per run.
    //
    // Additive and safe: with no shot waypoints assigned (HasRig == false) the
    // component is inert and EndingController behaves exactly as before. The
    // camera rail is a plain lerp between builder-placed transforms - no
    // Cinemachine dependency. VO plays through VoiceoverPlayer (Resources/VO/
    // <locale>/VO_<nodeId>); a missing clip is not an error, the line simply
    // shows as a subtitle until real recordings ship (generated placeholders
    // are acceptable per the current batch policy).
    public class EndingCinematicController : MonoBehaviour
    {
        [System.Serializable]
        public class Shot
        {
            public string id;
            [Tooltip("Camera pose at shot start (builder-placed waypoint).")]
            public Transform from;
            [Tooltip("Camera pose at shot end; the rail eases from -> to.")]
            public Transform to;
            [Tooltip("Total shot seconds. Lines divide it evenly (min per line applies).")]
            public float seconds = 7f;
            [Tooltip("Story node ids spoken/shown during this shot, in order.")]
            public string[] lineIds = new string[0];
            [Tooltip("Optional: subject pose at shot start (the on-screen character).")]
            public Transform subjectFrom;
            [Tooltip("Optional: subject pose at shot end; lerped with the camera.")]
            public Transform subjectTo;
        }

        public sealed class EndingLine
        {
            public string nodeId;
            public string clipId;
            public int speaker; // 0 Nour, 1 Mother, 2 Shadow, 3 Memory
            public string en;
            public string ar;
        }

        // The exact script from Docs/ENDING_VO_SPEC.md section 3. Clip ids
        // follow the line-sheet convention: VO_<nodeId>.
        public static readonly EndingLine[] Lines =
        {
            new EndingLine { nodeId = "SN_Ending_01", clipId = "VO_SN_Ending_01", speaker = 3,
                en = "The gate closes behind her. The sound is small: a latch, not a full stop.",
                ar = "يُغلَق الباب خلفها. الصوت صغير: طَقطَقة مِزلاج، مش نقطة في آخر السطر." },
            new EndingLine { nodeId = "SN_Ending_02", clipId = "VO_SN_Ending_02", speaker = 0,
                en = "Six pieces. Her handwriting. Her red pen. The mug with the crack she never threw away.",
                ar = "ست قطع. خط إيدها. قلمها الأحمر. الكوب اللي فيه شرخ ومقدرتش ترميه." },
            new EndingLine { nodeId = "SN_Ending_03", clipId = "VO_SN_Ending_03", speaker = 0,
                en = "I came back for a person, and the school tried to keep the receipt.",
                ar = "رجعت عشان أدوّر على شخص، والمدرسة حاولت تحتفظ بالإيصال." },
            new EndingLine { nodeId = "SN_Ending_04", clipId = "VO_SN_Ending_04", speaker = 1,
                en = "Knowledge is the only light that follows a person into the dark.",
                ar = "العِلم هو النور الوحيد اللي بيمشي مع الإنسان جوه الضلمة." },
            new EndingLine { nodeId = "SN_Ending_05", clipId = "VO_SN_Ending_05", speaker = 3,
                en = "On the pavement she opens the box. Nothing inside glows. Nothing needs to.",
                ar = "على الرصيف بتفتح العلبة. مفيش حاجة جواها بتنوّر. ومفيش حاجة محتاجة." },
            new EndingLine { nodeId = "SN_Ending_06", clipId = "VO_SN_Ending_06", speaker = 2,
                en = "You are leaving with… less than you arrived.",
                ar = "إنتي خراجة بـ… أقل من اللي جيتي بيه." },
            new EndingLine { nodeId = "SN_Ending_07", clipId = "VO_SN_Ending_07", speaker = 0,
                en = "No. I'm leaving with exactly what I came for. You're the one who's short.",
                ar = "لأ. أنا خراجة بالظبط باللي جيت عشانه. إنتي اللي ناقصك." },
            new EndingLine { nodeId = "SN_Ending_08", clipId = "VO_SN_Ending_08", speaker = 3,
                en = "The sky over Al-Noor turns from black to grey to a thin, clean gold.",
                ar = "السما فوق النور بتحوّل من الأسود للرمادي لدهبي رفيع ونضيف." },
            new EndingLine { nodeId = "SN_Ending_09", clipId = "VO_SN_Ending_09", speaker = 1,
                en = "Faster, habibti. The bell does not wait for the slow — and neither does the morning.",
                ar = "بسرعة يا حبيبتي. الجرس مش بيستنى البطيئين — والصبح برضه." },
            new EndingLine { nodeId = "SN_Ending_10", clipId = "VO_SN_Ending_10", speaker = 0,
                en = "I keep my promise out loud, because the school is listening, and schools keep records.",
                ar = "بقول وعدي بصوت عالي، عشان المدرسة سامعة، والمدارس بتحتفظ بالسجلات." },
            new EndingLine { nodeId = "SN_Ending_11", clipId = "VO_SN_Ending_11", speaker = 0,
                en = "I'll keep the light on. For the ones who come after.",
                ar = "هسيب النور والِّع. عشان اللي جايين بعديا." },
            new EndingLine { nodeId = "SN_Ending_12", clipId = "VO_SN_Ending_12", speaker = 2,
                en = "Then we will… remember you… differently.",
                ar = "إذن هنفـ… نفتكرك… بطريقة تانية." },
            new EndingLine { nodeId = "SN_Ending_13", clipId = "VO_SN_Ending_13", speaker = 3,
                en = "She walks into the morning without looking back. The windows of the school hold the dark a moment longer — then let it go.",
                ar = "بتمشي في الصبح من غير ما تبص وراها. شبابيك المدرسة ماسكين الضلمة لحظة كمان — وبعدين يسيبوها." },
            new EndingLine { nodeId = "SN_Ending_14", clipId = "VO_SN_Ending_14", speaker = 3,
                en = "Al-Noor keeps its records. Tonight, so did she.",
                ar = "النور بتحتفظ بسجلاتها. والليلة دي، هي كمان احتفظت." },
        };

        [Header("Shots (spec section 4; waypoints placed by the scene builder)")]
        public Shot[] shots = new Shot[0];

        [Header("Subject")]
        [Tooltip("The on-screen character (placeholder until the girl model ships). "
            + "Lerped between a shot's subjectFrom/subjectTo waypoints when assigned.")]
        public Transform subject;

        [Header("Timing")]
        public float minSecondsPerLine = 2.5f;
        [Tooltip("Short pause between shots so cuts can breathe.")]
        public float beatBetweenShots = 0.4f;

        [Header("Skip")]
        public bool skippable = true;

        public bool IsPlaying { get; private set; }

        // True only when the builder has placed at least one usable shot; an
        // unassigned controller leaves the ending scene untouched.
        public bool HasRig
        {
            get
            {
                if (shots == null) return false;
                for (int i = 0; i < shots.Length; i++)
                    if (shots[i] != null && shots[i].from != null && shots[i].to != null) return true;
                return false;
            }
        }

        public static EndingLine FindLine(string nodeId)
        {
            for (int i = 0; i < Lines.Length; i++)
                if (Lines[i].nodeId == nodeId) return Lines[i];
            return null;
        }

        // Plays the full sequence on the given camera, then restores the
        // camera's original pose so the dawn screen continues unaffected.
        public IEnumerator Play(Camera cam, Text subtitle)
        {
            if (cam == null || !HasRig) yield break;
            IsPlaying = true;

            Vector3 homePos = cam.transform.position;
            Quaternion homeRot = cam.transform.rotation;
            bool skipped = false;

            for (int s = 0; s < shots.Length && !skipped; s++)
            {
                Shot shot = shots[s];
                if (shot == null || shot.from == null || shot.to == null) continue;

                int lineCount = shot.lineIds != null ? shot.lineIds.Length : 0;
                if (lineCount == 0)
                {
                    yield return RunShotSegment(cam, shot, 0f, 1f, Mathf.Max(1f, shot.seconds));
                }
                else
                {
                    float perLine = Mathf.Max(minSecondsPerLine, shot.seconds / lineCount);
                    for (int li = 0; li < lineCount && !skipRequested; li++)
                    {
                        EndingLine line = FindLine(shot.lineIds[li]);
                        if (line == null) continue;

                        if (subtitle != null)
                        {
                            subtitle.text = Localization.Current == GameLanguage.Arabic ? Localization.Present(line.ar) : line.en;
                            subtitle.font = Localization.CurrentFont;
                            subtitle.alignment = Localization.Current == GameLanguage.Arabic
                                ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                        }
                        VoiceoverPlayer.Play(line.clipId); // false on missing clip: subtitle only

                        float t0 = (float)li / lineCount;
                        float t1 = (float)(li + 1) / lineCount;
                        yield return RunShotSegment(cam, shot, t0, t1, perLine);
                    }
                }
                skipped = skipRequested;

                if (beatBetweenShots > 0f && !skipped)
                    yield return new WaitForSecondsRealtime(beatBetweenShots);
            }

            VoiceoverPlayer.Stop();
            if (subtitle != null) subtitle.text = "";
            cam.transform.position = homePos;
            cam.transform.rotation = homeRot;
            IsPlaying = false;
            skipRequested = false;
        }

        bool skipRequested;

        static bool SkipPressed()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.Return);
        }

        // Eases the camera along the shot rail for `seconds`, mapping local
        // segment time onto the [t0, t1] slice of the shot's overall progress.
        IEnumerator RunShotSegment(Camera cam, Shot shot, float t0, float t1, float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (skippable && SkipPressed())
                {
                    skipRequested = true;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float local = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / seconds));
                float t = Mathf.Lerp(t0, t1, local);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                cam.transform.position = Vector3.Lerp(shot.from.position, shot.to.position, eased);
                cam.transform.rotation = Quaternion.Slerp(shot.from.rotation, shot.to.rotation, eased);
                if (subject != null && shot.subjectFrom != null && shot.subjectTo != null)
                {
                    subject.position = Vector3.Lerp(shot.subjectFrom.position, shot.subjectTo.position, eased);
                    subject.rotation = Quaternion.Slerp(shot.subjectFrom.rotation, shot.subjectTo.rotation, eased);
                }
                yield return null;
            }
        }
    }
}
