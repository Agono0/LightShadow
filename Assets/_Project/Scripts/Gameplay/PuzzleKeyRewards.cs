using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Keys through puzzles (user brief): the classroom and staff room keys are
    // earned, not found lying in corridors. Solving the chalkboard cipher pays
    // the classroom key, the map locker pays the staff room key, and the
    // first-aid cabinet restock pays a medkit. Listens to
    // GameEvents.PuzzleSolved (payload: puzzle id = the puzzle root's name)
    // and grants through Inventory, with one spoken line so the reward lands.
    public static class PuzzleKeyRewards
    {
        // puzzle id (PuzzleBase.puzzleId, defaulting to the root's name) ->
        // inventory item id.
        static readonly Dictionary<string, string> rewards = new Dictionary<string, string>
        {
            { "Puzzle_ChalkboardCipher", "classroom_key" },
            { "Puzzle_MapLocker", "staff_key" },
            { "Puzzle_FirstAidOrder", "medkit" },
        };

        public static bool TryGetReward(string puzzleId, out string itemId)
        {
            return rewards.TryGetValue(puzzleId ?? "", out itemId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            GameEvents.PuzzleSolved -= OnPuzzleSolved;
            GameEvents.PuzzleSolved += OnPuzzleSolved;
            Inventory.ItemRemoved -= OnItemRemoved;
            Inventory.ItemRemoved += OnItemRemoved;
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        static void OnPuzzleSolved(string puzzleId)
        {
            string itemId;
            if (!TryGetReward(puzzleId, out itemId)) return;
            if (Inventory.Has(itemId))
            {
                // One medkit carried (K1): the restock reward used to vanish when a kit
                // was already in the bag. Keep it owed instead and hand it over the
                // moment the carried one is used.
                if (itemId == MedkitPickup.ItemId && !pendingMedkit)
                {
                    pendingMedkit = true;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowMessage(
                            Localization.Current == GameLanguage.Arabic
                                ? "حقيبتك ممتلئة. ستحصل على حقيبة الخزانة بعد استخدام حقيبتك."
                                : "Your medkit slot is full. The cabinet's medkit is yours once you use yours.",
                            4f);
                    }
                }
                return;
            }

            Grant(itemId);
        }

        // A medkit the cabinet owes the player (the bag was full when it paid out).
        static bool pendingMedkit;
        public static bool PendingMedkit { get { return pendingMedkit; } }
        public const string PendingSaveKey = "pendingMedkit";

        public static void ResetPending() { pendingMedkit = false; }

        static void OnItemRemoved(string itemId)
        {
            if (!pendingMedkit || itemId != MedkitPickup.ItemId) return;
            // Not here: ItemRemoved fires inside the caller's own use (the health
            // code, then the inventory screen removes the same item again once the
            // handler returns). Granting now would let that second Remove delete the
            // new kit. Wait for the call stack to unwind.
            GrantRunner.Schedule();
        }

        sealed class GrantRunner : MonoBehaviour
        {
            static GrantRunner instance;

            public static void Schedule()
            {
                if (instance == null)
                {
                    GameObject host = new GameObject("PuzzleRewardRunner");
                    DontDestroyOnLoad(host);
                    instance = host.AddComponent<GrantRunner>();
                }
                instance.StartCoroutine(instance.GrantNextFrame());
            }

            System.Collections.IEnumerator GrantNextFrame()
            {
                yield return null;
                if (!pendingMedkit || Inventory.Has(MedkitPickup.ItemId)) yield break;
                pendingMedkit = false;
                Grant(MedkitPickup.ItemId);
            }
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            if (data != null && pendingMedkit) SaveSystem.SetContrib(data, PendingSaveKey, "1");
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            pendingMedkit = data != null && SaveSystem.GetContrib(data, PendingSaveKey) == "1";
        }

        static void Grant(string itemId)
        {
            Inventory.Add(itemId);

            // Same feedback as a physical pickup: sfx + the Taken toast.
            if (ProceduralSfx.Pickup != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(ProceduralSfx.Pickup);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? "أخذت" + ": " + Inventory.DisplayName(itemId)
                        : "Taken: " + Inventory.DisplayName(itemId),
                    3f);
            }
        }
    }
}
