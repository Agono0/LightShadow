using System;
using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Captions
{
    // Sound captions (package C3). One public call:
    //
    //     CaptionService.Raise("Footsteps nearby", worldPosition);
    //
    // Keys are English strings from captions.json (see CaptionKeys). The
    // caption shows in the player's language (EN or AR; English if the Arabic
    // field is empty) with a "<", ">" or "v" marker for left, right or
    // behind the camera, then fades. At most MaxVisible captions show at
    // once; further cues wait in a short queue and the oldest waiting cue is
    // dropped when the queue is full. The same key within DuplicateWindow
    // seconds is ignored so a looping sound does not flood the screen.
    //
    // Setting: CaptionService.Enabled, saved in the PlayerPrefs key
    // "abdelraouf.captions.enabled". Default ON. Raise does nothing while it
    // is off or while no CaptionService exists in the scene.
    //
    // Who calls Raise: the audio call sites in shared code are not part of
    // this package. This class only displays what it is told.
    public class CaptionService : MonoBehaviour
    {
        public const string PrefsKey = "abdelraouf.captions.enabled";

        public static CaptionService Instance { get; private set; }

        [Tooltip("Data/Captions/captions.json, assigned by the level builder.")]
        public TextAsset captionsJson;
        public int maxVisible = 3;
        public int maxQueued = 4;
        public float duplicateWindow = 0.75f;
        public float fadeSeconds = 0.6f;
        [Tooltip("Within this angle of straight ahead (degrees) no marker is shown.")]
        public float frontHalfAngle = 45f;
        [Tooltip("Beyond this angle from straight ahead (degrees) the cue is behind.")]
        public float behindAngle = 135f;

        // On by default. A missing PlayerPrefs key reads as on.
        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(PrefsKey, 1) != 0; }
            set
            {
                PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0);
                PlayerPrefs.Save();
                if (!value && Instance != null) Instance.ClearAll();
            }
        }

        // Public one-line API. Safe to call from anywhere, any time.
        public static void Raise(string englishKey, Vector3 worldPos)
        {
            RaiseInternal(englishKey, worldPos, false);
        }

        internal static void RaiseInternal(string englishKey, Vector3 worldPos, bool simulated)
        {
            if (Instance == null || !Enabled) return;
            if (string.IsNullOrEmpty(englishKey)) return;
            Instance.Enqueue(englishKey, worldPos, simulated);
        }

        class Active
        {
            public string key;
            public CaptionCue cue;
            public Vector3 pos;
            public float born;
            public float life;
            public bool simulated;
        }

        readonly List<Active> shown = new List<Active>();
        readonly List<Active> waiting = new List<Active>();
        readonly Dictionary<string, float> lastRaised = new Dictionary<string, float>();
        readonly List<CaptionUI.Row> rows = new List<CaptionUI.Row>();
        CaptionUI ui;

        void Awake()
        {
            Instance = this;
            ui = gameObject.AddComponent<CaptionUI>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Enqueue(string key, Vector3 pos, bool simulated)
        {
            float now = Time.unscaledTime;
            float last;
            if (lastRaised.TryGetValue(key, out last) && now - last < duplicateWindow) return;
            lastRaised[key] = now;

            CaptionCue cue = CaptionCatalog.Find(captionsJson != null ? captionsJson.text : "", key);
            var entry = new Active
            {
                key = key,
                cue = cue,
                pos = pos,
                life = cue != null && cue.seconds > 0.5f ? cue.seconds : 2.5f,
                simulated = simulated,
            };

            if (shown.Count < Mathf.Max(1, maxVisible))
            {
                entry.born = now;
                shown.Add(entry);
            }
            else
            {
                waiting.Add(entry);
                while (waiting.Count > Mathf.Max(0, maxQueued)) waiting.RemoveAt(0);
            }
        }

        void ClearAll()
        {
            shown.Clear();
            waiting.Clear();
            rows.Clear();
            if (ui != null) ui.SetRows(rows);
        }

        void Update()
        {
            float now = Time.unscaledTime;

            for (int i = shown.Count - 1; i >= 0; i--)
            {
                if (now - shown[i].born >= shown[i].life) shown.RemoveAt(i);
            }
            while (shown.Count < Mathf.Max(1, maxVisible) && waiting.Count > 0)
            {
                Active next = waiting[0];
                waiting.RemoveAt(0);
                next.born = now;
                shown.Add(next);
            }

            rows.Clear();
            if (Enabled)
            {
                bool arabic = Localization.Current == GameLanguage.Arabic;
                Camera cam = Camera.main;
                for (int i = 0; i < shown.Count; i++)
                {
                    Active item = shown[i];
                    float age = now - item.born;
                    float remaining = item.life - age;
                    float alpha = Mathf.Clamp01(Mathf.Min(remaining / Mathf.Max(0.05f, fadeSeconds), age / 0.15f + 0.01f));
                    rows.Add(new CaptionUI.Row
                    {
                        text = TextFor(item, arabic),
                        marker = MarkerFor(cam, item.pos),
                        alpha = alpha,
                    });
                }
            }
            ui.SetRows(rows);
        }

        static string TextFor(Active item, bool arabic)
        {
            string text;
            if (item.cue != null)
            {
                text = arabic && !string.IsNullOrEmpty(item.cue.textAr) ? item.cue.textAr : item.cue.textEn;
                if (string.IsNullOrEmpty(text)) text = item.key;
            }
            else
            {
                text = item.key;
            }
            if (arabic && item.cue != null && !string.IsNullOrEmpty(item.cue.textAr)) text = Localization.Present(text);
            return item.simulated ? "[SIM] " + text : text;
        }

        char MarkerFor(Camera cam, Vector3 worldPos)
        {
            if (cam == null) return ' ';
            Vector3 toCue = worldPos - cam.transform.position;
            toCue.y = 0f;
            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            if (toCue.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f) return ' ';
            float angle = Vector3.SignedAngle(forward, toCue, Vector3.up); // positive = right
            float abs = Mathf.Abs(angle);
            if (abs >= behindAngle) return 'v';
            if (abs <= frontHalfAngle) return ' ';
            return angle > 0f ? '>' : '<';
        }
    }
}
