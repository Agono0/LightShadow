using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LightShadow.Player;
using UnityEngine;
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

        // ---------------------------------------------------------------- io --

        static string SlotPath(int slot)
        {
            return Path.Combine(Application.persistentDataPath,
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

        public static SaveData LoadFromSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount || !HasSave(slot)) return null;

            try
            {
                string json = AtomicFile.ReadAllText(SlotPath(slot), LooksLikeJsonObject);
                return json == null ? null : JsonUtility.FromJson<SaveData>(json);
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
            string newestStamp = null;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!HasSave(slot)) continue;
                SaveData data = LoadFromSlot(slot);
                if (data == null) continue;
                if (newestStamp == null || string.CompareOrdinal(data.savedAtUtc, newestStamp) > 0)
                {
                    newestStamp = data.savedAtUtc;
                    newest = slot;
                }
            }
            return newest;
        }

        // -------------------------------------------------------- capture --

        // Builds a SaveData from the live game. Returns null outside a run
        // (bootstrap, main menu, ending).
        public static SaveData Capture()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.player == null) return null;
            if (SceneManager.GetActiveScene().name != SceneNames.School) return null;

            EnsureSession();

            SaveData data = new SaveData
            {
                version = 1,
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
            SaveCaptured?.Invoke(data);
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
                SaveApplied?.Invoke(data);
                return;
            }

            // The scene is still loading (or the receiver spawns late): wait.
            pendingRestore = data;
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
        }

        // Drops a deferred restore that never matched its scene. Called when
        // the player starts a new game, so a stale save cannot land in the
        // fresh run when the school scene loads again (audit A3).
        public static void ClearPendingRestore()
        {
            pendingRestore = null;
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
            SaveData data = pendingRestore;
            pendingRestore = null;

            GameManager game = GameManager.Instance;
            if (game == null || game.player == null)
            {
                Debug.LogWarning("[LightShadow] Save restored late: no GameManager/player in " + scene.name);
                return;
            }

            game.ApplyLoadedState(data);
            SaveApplied?.Invoke(data);
        }

        static void EnsureSession()
        {
            if (sessionStart < 0.0) sessionStart = Time.realtimeSinceStartupAsDouble;
        }
    }
}
