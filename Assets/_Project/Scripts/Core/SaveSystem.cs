using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LightShadow.Core
{
    // Three JSON save slots under the player's persistent data folder
    // (decision D12). The file format is a flat SaveData blob written with
    // JsonUtility - no external dependency, easy to debug, easy to migrate
    // (the version field guards future changes).
    //
    // The contract below is frozen: the Narrative agent codes against these
    // exact names. storyJson is Narrative's StoryDirector state blob - the
    // engine copies it verbatim and never interprets it (see SaveCaptured /
    // SaveApplied below for the two injection points).
    public static class SaveSystem
    {
        public const int SlotCount = 3;

        // The slot every write goes to until the menu UI changes it (0-2).
        public static int CurrentSlot { get; set; } = 0;

        // Raised just before a SaveData blob is written. Subscribers (the story
        // system) fill in their state on the object passed in.
        public static event Action<SaveData> SaveCaptured;

        // Raised after a save has been applied to the live game (position, meter,
        // fuses, gate...). Subscribers restore their own state from storyJson here.
        public static event Action<SaveData> SaveApplied;

        [Serializable]
        public class SaveData
        {
            public int version = 1;
            public int layoutVersion;          // 0 = saved before layout versioning (treated as compatible)
            public string sceneName;
            public Vector3 position;
            public float yaw;
            public float meterValue;
            public float batteryFraction;
            public bool flashlightOn;
            public int fusesRestored;
            public int fragmentsCollected;
            public bool gatePowered;
            public string storyJson = "";      // Narrative's StoryDirector state blob
            public string puzzleJson = "";     // PuzzleManager blob: solved wing-puzzle ids
            public string inventoryJson = "";  // Inventory blob: carried item ids
            public float playSeconds;
            public string savedAtUtc;

            // Per-feature blobs for contributor modules (contract H1). Old
            // saves lack the field and deserialize to an empty list.
            public List<ContribEntry> contrib = new List<ContribEntry>();
        }

        [Serializable]
        public class ContribEntry
        {
            public string key;
            public string json;
        }

        // Contract H1. Feature modules own one key each ("journal", "hints"...)
        // and put any JSON string they like under it; the engine never reads it.
        // Call SetContrib from a SaveCaptured handler and GetContrib from a
        // SaveApplied handler. GetContrib returns "" when nothing was saved.
        public static void SetContrib(SaveData data, string key, string json)
        {
            if (data == null || string.IsNullOrEmpty(key)) return;
            if (data.contrib == null) data.contrib = new List<ContribEntry>();
            for (int i = 0; i < data.contrib.Count; i++)
            {
                if (data.contrib[i] != null && data.contrib[i].key == key)
                {
                    data.contrib[i].json = json ?? "";
                    return;
                }
            }
            data.contrib.Add(new ContribEntry { key = key, json = json ?? "" });
        }

        public static string GetContrib(SaveData data, string key)
        {
            if (data == null || data.contrib == null || string.IsNullOrEmpty(key)) return "";
            for (int i = 0; i < data.contrib.Count; i++)
            {
                if (data.contrib[i] != null && data.contrib[i].key == key) return data.contrib[i].json ?? "";
            }
            return "";
        }

        const string FolderName = "lightshadow/saves";
        const string FilePrefix = "slot";
        const string FileExtension = ".json";
        const string TimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        // Session clock for playSeconds. It counts the current play session
        // (bootstrap to quit), not a lifetime total - documented honestly.
        static double sessionStart = -1.0;

        // Pending restore waiting for the right scene to finish loading.
        static SaveData pendingRestore;

        // The opening line and narration belong to a new night only. A
        // restore is either still pending when the school starts, or has
        // already been applied; both mean "do not replay the intro".
        static bool restoredThisRun;
        public static bool RestorePending { get { return pendingRestore != null; } }

        public static bool ConsumeRestoredFlag()
        {
            bool value = restoredThisRun;
            restoredThisRun = false;
            return value;
        }

        // Saved positions can land inside geometry that moved since the save
        // (or a rebaked layout). Project x/z onto the NavMesh within maxDistance;
        // y stays as saved. False when there is no NavMesh or nothing close.
        public static bool TryProjectToNavMesh(Vector3 point, float maxDistance, out Vector3 projected)
        {
            projected = point;
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(point, out hit, maxDistance, NavMesh.AllAreas)) return false;
            projected = new Vector3(hit.position.x, point.y, hit.position.z);
            return true;
        }

        // ---------------------------------------------------------------- io --

        // Test seam: tests point saves at a temp folder so they never touch real
        // slots. null (the default) keeps Application.persistentDataPath.
        public static string RootDirectoryOverride { get; set; }

        public static void ResetRootDirectoryOverride() { RootDirectoryOverride = null; }

        static string RootDirectory()
        {
            return string.IsNullOrEmpty(RootDirectoryOverride) ? Application.persistentDataPath : RootDirectoryOverride;
        }

        static string SlotPath(int slot)
        {
            return Path.Combine(RootDirectory(),
                FolderName, FilePrefix + slot + FileExtension);
        }

        public static bool SaveToSlot(int slot, SaveData data)
        {
            if (data == null || slot < 0 || slot >= SlotCount) return false;

            try
            {
                string directory = Path.GetDirectoryName(SlotPath(slot));
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                AtomicFile.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Save failed for slot " + slot + ": " + exception.Message);
                return false;
            }
        }

        static bool LooksLikeJsonObject(string text)
        {
            string trimmed = text.Trim();
            return trimmed.StartsWith("{") && trimmed.EndsWith("}");
        }

        static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }

        // S7: a damaged or hand-edited file must not teleport the player to NaN
        // or the far side of the world. The old scene-layout check (NavMesh) is
        // not done: it needs a device to judge.
        // Bump LayoutVersion whenever the school's walls or rooms move, so an
        // old save's position is not restored inside new geometry.
        public const int LayoutVersion = 1;

        public static bool HasUsablePosition(SaveData data)
        {
            if (data == null) return false;
            if (data.layoutVersion > 0 && data.layoutVersion != LayoutVersion) return false;
            Vector3 p = data.position;
            const float Limit = 100000f;
            return Finite(p.x) && Finite(p.y) && Finite(p.z)
                && Mathf.Abs(p.x) < Limit && Mathf.Abs(p.y) < Limit && Mathf.Abs(p.z) < Limit;
        }

        // S9: non-finite scalars fall back to safe defaults; finite ones are
        // clamped to their legal range.
        public static void Sanitize(SaveData data)
        {
            if (data == null) return;
            if (!Finite(data.yaw)) data.yaw = 0f;
            data.meterValue = Finite(data.meterValue) ? data.meterValue : 60f;
            data.batteryFraction = Finite(data.batteryFraction) ? Mathf.Clamp01(data.batteryFraction) : 1f;
            if (!Finite(data.playSeconds) || data.playSeconds < 0f) data.playSeconds = 0f;
        }

        public static SaveData LoadFromSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount || !HasSave(slot)) return null;

            try
            {
                string json = AtomicFile.ReadAllText(SlotPath(slot), LooksLikeJsonObject);
                if (json == null) return null;
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                Sanitize(loaded);
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Load failed for slot " + slot + ": " + exception.Message);
                return null;
            }
        }

        public static bool HasSave(int slot)
        {
            if (slot < 0 || slot >= SlotCount) return false;
            return File.Exists(SlotPath(slot));
        }

        public static void DeleteSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount) return;
            try
            {
                AtomicFile.Delete(SlotPath(slot));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Delete failed for slot " + slot + ": " + exception.Message);
            }
        }

        // The slot with the most recent save, or -1 when nothing was ever saved.
        public static int NewestSlot()
        {
            int newest = -1;
            string newestStamp = "";
            int unstamped = -1; // a readable slot with no stamp: only used when nothing is stamped
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!HasSave(slot)) continue;
                SaveData data = LoadFromSlot(slot);
                if (data == null) continue;
                if (string.IsNullOrEmpty(data.savedAtUtc))
                {
                    if (unstamped < 0) unstamped = slot;
                    continue;
                }
                if (newest < 0 || string.CompareOrdinal(data.savedAtUtc, newestStamp) > 0)
                {
                    newestStamp = data.savedAtUtc;
                    newest = slot;
                }
            }
            return newest >= 0 ? newest : unstamped;
        }

        // -------------------------------------------------------- capture --

        // Builds a SaveData from the live game. Returns null outside a run
        // (bootstrap, main menu, ending).
        public static SaveData Capture()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.player == null) return null;
            if (SceneManager.GetActiveScene().name != SceneNames.School) return null;
            // Mid catch fade: keep the previous good save rather than the death spot.
            if (game.IsRespawning) return null;

            EnsureSession();

            SaveData data = new SaveData
            {
                version = 1,
                layoutVersion = LayoutVersion,
                sceneName = SceneManager.GetActiveScene().name,
                position = game.player.position,
                yaw = game.player.eulerAngles.y,
                meterValue = 60f,
                batteryFraction = 1f,
                flashlightOn = false,
                fusesRestored = game.FusesRestored,
                fragmentsCollected = game.FragmentsCollected,
                gatePowered = game.GatePowered,
                storyJson = "",
                playSeconds = (float)(Time.realtimeSinceStartupAsDouble - sessionStart),
                savedAtUtc = DateTime.UtcNow.ToString(TimeFormat, CultureInfo.InvariantCulture),
            };

            BrightIdeaMeter meter = game.GetMeter();
            if (meter != null) data.meterValue = meter.Meter;

            FlashlightBattery flashlight = game.player.GetComponentInChildren<FlashlightBattery>();
            if (flashlight != null)
            {
                data.batteryFraction = flashlight.Battery01;
                data.flashlightOn = flashlight.IsOn;
            }

            // Narrative fills storyJson here, before the blob is written.
            InvokeEach(SaveCaptured, data, "SaveCaptured");
            return data;
        }

        // Checkpoint/gate autosave into the current slot. Safe to call from
        // anywhere: it silently does nothing outside a school run.
        public static void Autosave()
        {
            SaveData data = Capture();
            if (data == null) return;
            if (SaveToSlot(CurrentSlot, data))
            {
                Debug.Log("[LightShadow] Autosaved to slot " + CurrentSlot + " at " + data.savedAtUtc);
            }
        }

        // -------------------------------------------------------- restore --

        // Applies a save to the live game. If the saved scene is not loaded yet,
        // the application is deferred until that scene finishes loading (the
        // menu loads the scene first, then calls this).
        public static void Restore(SaveData data)
        {
            if (data == null || string.IsNullOrEmpty(data.sceneName))
            {
                Debug.LogWarning("[LightShadow] Restore called with an empty save.");
                return;
            }

            GameManager game = GameManager.Instance;
            bool sceneMatches = SceneManager.GetActiveScene().name == data.sceneName;
            bool hasReceiver = game != null && game.player != null;

            if (sceneMatches && hasReceiver)
            {
                game.ApplyLoadedState(data);
                NotifySaveApplied(data);
                return;
            }

            // The scene is still loading (or the receiver spawns late): wait.
            pendingRestore = data;
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;

            // S6: when the scene is already the right one, sceneLoaded has
            // already fired, so also poll for the receiver for a few seconds.
            if (sceneMatches) StartRestoreRetry();
        }

        const float RestoreRetrySeconds = 10f;

        sealed class RestoreRetry : MonoBehaviour
        {
            float started;

            void Awake() { started = Time.unscaledTime; }

            void Update()
            {
                if (pendingRestore == null) { Destroy(gameObject); return; }
                GameManager game = GameManager.Instance;
                if (game != null && game.player != null)
                {
                    ApplyPending();
                    Destroy(gameObject);
                    return;
                }
                if (Time.unscaledTime - started > RestoreRetrySeconds)
                {
                    Debug.LogWarning("[LightShadow] Save restore gave up: no GameManager/player after " + RestoreRetrySeconds + "s.");
                    ClearPendingRestore();
                    Destroy(gameObject);
                }
            }
        }

        static void StartRestoreRetry()
        {
            if (!Application.isPlaying) return;
            if (UnityEngine.Object.FindFirstObjectByType<RestoreRetry>() != null) return;
            new GameObject("SaveRestoreRetry").AddComponent<RestoreRetry>();
        }

        static void ApplyPending()
        {
            SaveData data = pendingRestore;
            pendingRestore = null;
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            GameManager game = GameManager.Instance;
            if (data == null || game == null || game.player == null) return;
            game.ApplyLoadedState(data);
            NotifySaveApplied(data);
        }

        // S2: Android can kill a backgrounded app, so save when it is paused or
        // quit. Mobile only (a desktop or editor run must not overwrite a slot
        // on focus loss); Capture() returns null outside a school run.
        sealed class LifecycleSaver : MonoBehaviour
        {
            void OnApplicationPause(bool paused) { if (paused) Autosave(); }
            void OnApplicationQuit() { Autosave(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void HookLifecycle()
        {
            if (!Application.isMobilePlatform) return;
            var host = new GameObject("SaveLifecycleSaver");
            host.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<LifecycleSaver>();
        }

        // Drops a deferred restore that never matched its scene. Called when
        // the player starts a new game, so a stale save cannot land in the
        // fresh run when the school scene loads again (audit A3).
        public static void ClearPendingRestore()
        {
            pendingRestore = null;
            restoredThisRun = false;
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        }

        static void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
        {
            if (pendingRestore == null)
            {
                SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
                return;
            }

            if (scene.name != pendingRestore.sceneName) return;

            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;

            GameManager game = GameManager.Instance;
            if (game == null || game.player == null)
            {
                // The receiver spawns after sceneLoaded: keep the save and poll.
                Debug.LogWarning("[LightShadow] Save restore waiting for GameManager/player in " + scene.name);
                StartRestoreRetry();
                return;
            }

            ApplyPending();
        }

        // Inventory and PuzzleManager restore inside SaveApplied and raise no
        // item or puzzle events, and ApplyLoadedState refreshes the HUD before
        // they run, so the objective line could stay stale after a deferred
        // restore. Refresh once more after every handler has run.
        public static void NotifySaveApplied(SaveData data)
        {
            restoredThisRun = true;
            InvokeEach(SaveApplied, data, "SaveApplied");
            LightShadow.UI.HUDController hud = LightShadow.UI.HUDController.Instance;
            if (hud == null) return;
            try { hud.RefreshObjectives(); }
            catch (Exception e) { Debug.LogException(e); }
        }

        // S3: one throwing subscriber must not skip the others or lose the save.
        static void InvokeEach(Action<SaveData> handlers, SaveData data, string label)
        {
            if (handlers == null) return;
            Delegate[] list = handlers.GetInvocationList();
            for (int i = 0; i < list.Length; i++)
            {
                try { ((Action<SaveData>)list[i])(data); }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] " + label + " handler failed: " + exception.Message);
                }
            }
        }

        static void EnsureSession()
        {
            if (sessionStart < 0.0) sessionStart = Time.realtimeSinceStartupAsDouble;
        }
    }
}
