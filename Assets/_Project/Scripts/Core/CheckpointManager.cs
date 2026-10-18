using UnityEngine;

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
        }
    }
}
