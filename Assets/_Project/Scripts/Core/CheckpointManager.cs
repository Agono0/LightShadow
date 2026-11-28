using System;
using UnityEngine;
using LightShadow.Gameplay;

namespace LightShadow.Core
{
    // Remembers the last lit checkpoint so being caught sends Nour back there
    // instead of to the start (Doc 4.3: "wakes at the last lit checkpoint").
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Wiring")]
        public Transform defaultSpawn;

        public Transform SpawnPoint { get; private set; }
        public string ChalkLine { get; private set; } = "";

        public const string SaveKey = "checkpoint";

        [Serializable]
        public class CheckpointSave
        {
            public bool lit;
            public string zoneName = "";
            public Vector3 position;
            public string chalkLine = "";
        }

        // True once a lit zone (not the start) is the wake point.
        public bool HasLitCheckpoint => SpawnPoint != null && SpawnPoint != defaultSpawn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            CheckpointManager manager = Instance;
            if (data == null || manager == null) return;
            SaveSystem.SetContrib(data, SaveKey, JsonUtility.ToJson(manager.Capture()));
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            CheckpointManager manager = Instance;
            if (data == null || manager == null) return;
            manager.RestoreFromJson(SaveSystem.GetContrib(data, SaveKey));
        }

        public CheckpointSave Capture()
        {
            CheckpointSave save = new CheckpointSave();
            if (!HasLitCheckpoint) return save;
            save.lit = true;
            save.zoneName = SpawnPoint.name;
            save.position = SpawnPoint.position;
            save.chalkLine = ChalkLine ?? "";
            return save;
        }

        // Picks the lit zone a save names: same name, nearest to the saved
        // position. Returns -1 when none matches (the zone was removed).
        public static int PickZone(string[] names, Vector3[] positions, string wanted, Vector3 position)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            if (names == null || positions == null || string.IsNullOrEmpty(wanted)) return -1;
            int count = Mathf.Min(names.Length, positions.Length);
            for (int i = 0; i < count; i++)
            {
                if (names[i] != wanted) continue;
                float d = (positions[i] - position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        // Restores quietly: no toast, no event, no autosave (R2). Old saves and
        // saves from before any lit zone fall back to the start spawn.
        public void RestoreFromJson(string json)
        {
            SpawnPoint = defaultSpawn;
            ChalkLine = "";
            if (string.IsNullOrEmpty(json)) return;
            CheckpointSave save = null;
            try { save = JsonUtility.FromJson<CheckpointSave>(json); }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Checkpoint save blob unreadable, using the start spawn: " + exception.Message);
            }
            if (save == null || !save.lit) return;

            LitZone[] zones = FindObjectsByType<LitZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            string[] names = new string[zones.Length];
            Vector3[] positions = new Vector3[zones.Length];
            for (int i = 0; i < zones.Length; i++)
            {
                names[i] = zones[i].isCheckpoint ? zones[i].transform.name : "";
                positions[i] = zones[i].transform.position;
            }
            int pick = PickZone(names, positions, save.zoneName, save.position);
            if (pick < 0) return;
            SpawnPoint = zones[pick].transform;
            ChalkLine = save.chalkLine ?? "";
        }

        void Awake()
        {
            Instance = this;
            SpawnPoint = defaultSpawn;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetCheckpoint(Transform point, string chalkLine)
        {
            if (point == null) return;
            if (point == SpawnPoint) return;

            SpawnPoint = point;
            ChalkLine = chalkLine;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    string.IsNullOrEmpty(chalkLine) ? "Checkpoint reached." : chalkLine,
                    4f);
            }

            // Event bus + autosave: every lit-zone checkpoint is a save point.
            GameEvents.RaiseCheckpointReached(string.IsNullOrEmpty(chalkLine) ? "Checkpoint reached." : chalkLine);
            SaveSystem.Autosave();
        }
    }
}
