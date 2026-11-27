using System;
using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Journal;
using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Hints
{
    // Progressive hints (package C2): nudge, clue, answer, per puzzle.
    //
    // How it finds the puzzle (read-only, no edit to PuzzleBase/PuzzleManager):
    // every PuzzleBase in the scene exposes a public puzzleId and State.
    // The "current" puzzle is the nearest PuzzleBase whose State is Available
    // and that has a row in hints.json, within MaxDistance of the camera.
    // PuzzleBase falls back to the GameObject name for puzzleId, and the scene
    // builder does not set it, so the real ids are the object names
    // ("Puzzle_ChalkboardCipher" and so on). See SUPPORT_SYSTEMS.md.
    //
    // Triggers: the H key, or no player input for IdleSeconds. A cooldown of
    // CooldownSeconds sits between any two hints. Each trigger moves the
    // current puzzle one level up and never past "answer".
    //
    // State: shown levels per puzzle and the total count are saved through
    // contract H1 under the key "hints". GameEvents.PuzzleSolved clears that
    // puzzle's levels and hides the banner.
    public class HintService : MonoBehaviour
    {
        public const string SaveKey = "hints";
        public const string PrefsKey = "abdelraouf.hints.enabled";

        // On by default. A missing PlayerPrefs key reads as on.
        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(PrefsKey, 1) != 0; }
            set { PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }
        public const int MaxLevel = 3;

        public static HintService Instance { get; private set; }

        [Tooltip("Data/Hints/hints.json, assigned by the level builder.")]
        public TextAsset hintsJson;
        public KeyCode hintKey = KeyCode.H;
        public float idleSeconds = 75f;
        public float cooldownSeconds = 20f;
        public float maxDistance = 8f;
        public float showSeconds = 9f;

        static readonly Dictionary<string, int> levels = new Dictionary<string, int>();
        static int hintsUsed;

        [Serializable]
        class HintSave
        {
            public int total;
            public List<string> ids = new List<string>();
            public List<int> levels = new List<int>();
        }

        // Number of hints shown this run. For an end-of-run summary to read;
        // RunResults is not edited from the contributor lane.
        public static int HintsUsed => hintsUsed;

        // Raised after a hint is shown: (puzzleId, level 1 to 3).
        public static event Action<string, int> HintShown;

        public static int LevelFor(string puzzleId)
        {
            int level;
            return !string.IsNullOrEmpty(puzzleId) && levels.TryGetValue(puzzleId, out level) ? level : 0;
        }

        HintUI ui;
        float lastActivity;
        float lastHintTime = -1000f;
        float nextScan;
        PuzzleBase current;
        Vector3 lastMouse;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            levels.Clear();
            hintsUsed = 0;
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        void Awake()
        {
            Instance = this;
            // Fresh scene, fresh hint state. A save restore arrives later, from
            // the sceneLoaded callback, and overwrites this.
            levels.Clear();
            hintsUsed = 0;
            ui = gameObject.AddComponent<HintUI>();
            lastActivity = Time.unscaledTime;
            lastMouse = Input.mousePosition;
        }

        void OnEnable()
        {
            GameEvents.PuzzleSolved += OnPuzzleSolved;
        }

        void OnDisable()
        {
            GameEvents.PuzzleSolved -= OnPuzzleSolved;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (PauseMenu.IsOpen || PlayerController.ExternalInputLocked || !Enabled)
            {
                lastActivity = Time.unscaledTime;
                return;
            }

            if (Input.GetKeyDown(hintKey))
            {
                lastActivity = Time.unscaledTime;
                RequestHint();
                return;
            }

            Vector3 mouse = Input.mousePosition;
            bool moved = (mouse - lastMouse).sqrMagnitude > 4f;
            lastMouse = mouse;
            // Gamepad sticks do not set anyKey, so read the move and look axes too.
            bool stick = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.2f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.2f;
            if (Input.anyKey || moved || stick || Input.touchCount > 0)
            {
                lastActivity = Time.unscaledTime;
                return;
            }

            if (Time.unscaledTime - lastActivity >= idleSeconds)
            {
                lastActivity = Time.unscaledTime;
                RequestHint();
            }
        }

        // Public so a touch button or another system can ask for a hint for
        // whichever puzzle is nearest. Returns true when a hint was shown.
        public bool RequestHint()
        {
            if (!Enabled) return false;
            if (Time.unscaledTime - lastHintTime < cooldownSeconds) return false;
            PuzzleBase puzzle = FindCurrentPuzzle();
            if (puzzle == null) return false;
            return RequestHintFor(puzzle.puzzleId);
        }

        // Shows the next level for one puzzle id. Cooldown applies.
        public bool RequestHintFor(string puzzleId)
        {
            if (string.IsNullOrEmpty(puzzleId)) return false;
            if (Time.unscaledTime - lastHintTime < cooldownSeconds) return false;
            HintEntry entry = HintCatalog.Find(hintsJson != null ? hintsJson.text : "", puzzleId);
            if (entry == null) return false;

            int level = Mathf.Min(LevelFor(puzzleId) + 1, MaxLevel);
            levels[puzzleId] = level;
            hintsUsed++;
            lastHintTime = Time.unscaledTime;
            ShowLevel(entry, level);
            Action<string, int> handler = HintShown;
            if (handler != null)
            {
                try { handler(puzzleId, level); }
                catch (Exception exception) { Debug.LogWarning("[LightShadow] Hint listener failed: " + exception.Message); }
            }
            return true;
        }

        // Debug-rig path: shows a level of a built-in stub entry. It does not
        // count, does not save and ignores the cooldown. Only compiled into
        // editor and development builds.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int stubLevel;

        public void ShowStubHint()
        {
            var stub = new HintEntry
            {
                puzzleId = "debug_stub",
                nudgeEn = "[TEST] Nudge: look closely at what is in front of you.",
                clueEn = "[TEST] Clue: one thing here does not belong.",
                answerEn = "[TEST] Answer: this is a stub puzzle, there is nothing to solve.",
            };
            stubLevel = stubLevel % MaxLevel + 1;
            ShowLevel(stub, stubLevel);
        }
#endif

        // The counter ("1/3") goes outside the shaped Arabic: the Arabic path
        // reverses whole lines, which turns digit runs backwards. The label is
        // drawn left to right, so the number sits on the left of the shaped
        // name and still reads 1/3. Pure so EditMode can test it. The visual
        // result in the game is not checked.
        public static string BuildTitle(int level, int maxLevel, bool arabic, string nameEn, string nameAr,
            System.Func<string, string> present)
        {
            if (!arabic) return "Hint " + level + "/" + maxLevel + " - " + nameEn;
            string shaped = present != null ? present(nameAr) : nameAr;
            return level + "/" + maxLevel + " " + shaped;
        }

        void ShowLevel(HintEntry entry, int level)
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            string[] namesEn = { "Nudge", "Clue", "Answer" };
            string[] namesAr = { "تلميح خفيف", "دليل", "الحل" };
            string title = BuildTitle(level, MaxLevel, arabic, namesEn[level - 1], namesAr[level - 1], Localization.Present);
            string body = entry.Get(level, arabic);
            if (arabic && !string.IsNullOrEmpty(entry.Get(level, true)))
            {
                body = Localization.Present(JournalBook.WrapLogical(body, 46));
            }
            if (ui != null) ui.Show(title, body, showSeconds);
        }

        PuzzleBase FindCurrentPuzzle()
        {
            if (Time.unscaledTime >= nextScan || current == null)
            {
                nextScan = Time.unscaledTime + 1f;
                current = null;
                Camera cam = Camera.main;
                if (cam == null) return null;
                string json = hintsJson != null ? hintsJson.text : "";
                PuzzleBase[] all = FindObjectsByType<PuzzleBase>(FindObjectsSortMode.None);
                float best = maxDistance * maxDistance;
                for (int i = 0; i < all.Length; i++)
                {
                    PuzzleBase puzzle = all[i];
                    if (puzzle == null || puzzle.State != PuzzleBase.PuzzleState.Available) continue;
                    if (HintCatalog.Find(json, puzzle.puzzleId) == null) continue;
                    float sqr = (puzzle.transform.position - cam.transform.position).sqrMagnitude;
                    if (sqr <= best)
                    {
                        best = sqr;
                        current = puzzle;
                    }
                }
            }
            if (current != null && current.State != PuzzleBase.PuzzleState.Available) current = null;
            return current;
        }

        void OnPuzzleSolved(string puzzleId)
        {
            if (!string.IsNullOrEmpty(puzzleId)) levels.Remove(puzzleId);
            current = null;
            nextScan = 0f;
            if (ui != null) ui.Hide();
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            try
            {
                var save = new HintSave { total = hintsUsed };
                foreach (KeyValuePair<string, int> pair in levels)
                {
                    save.ids.Add(pair.Key);
                    save.levels.Add(pair.Value);
                }
                SaveSystem.SetContrib(data, SaveKey, JsonUtility.ToJson(save));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Hints could not write their save blob: " + exception.Message);
            }
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            levels.Clear();
            hintsUsed = 0;
            try
            {
                string json = SaveSystem.GetContrib(data, SaveKey);
                if (string.IsNullOrEmpty(json)) return;
                HintSave save = JsonUtility.FromJson<HintSave>(json);
                if (save == null) return;
                hintsUsed = Mathf.Max(0, save.total);
                int count = save.ids != null && save.levels != null ? Mathf.Min(save.ids.Count, save.levels.Count) : 0;
                for (int i = 0; i < count; i++)
                {
                    if (string.IsNullOrEmpty(save.ids[i])) continue;
                    levels[save.ids[i]] = Mathf.Clamp(save.levels[i], 0, MaxLevel);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Hint save blob unreadable, starting with no hints used: " + exception.Message);
            }
        }
    }
}
