using System;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Player
{
    // Two-hit health (user brief 2026-11-27, fixed medium difficulty): the
    // wraith's first strike takes half, the second ends the run and Nour wakes
    // at the chapter checkpoint with full health. No passive regen - medkits
    // (rare, mostly puzzle rewards) are the only way back to full.
    public class PlayerHealth : MonoBehaviour
    {
        public const int MaxHits = 2;

        public static PlayerHealth Instance { get; private set; }

        // 2 = untouched, 1 = half, 0 = dead (Died fires on the lethal strike;
        // GameManager resets health on the checkpoint wake).
        public int HitsRemaining { get; private set; } = MaxHits;

        public float Normalized => (float)HitsRemaining / MaxHits;

        public event Action<float> Changed; // normalized, for the HUD bar
        public event Action Hit;            // survived strike (feedback)
        public event Action Died;           // lethal strike

        // Save bridge (audit: health was not saved, so a reload healed). One
        // contrib key; an old save without it restores full health.
        public const string SaveKey = "player_health";
        static int pendingHits = -1; // -1: nothing waiting to be applied

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void HookSaves()
        {
            pendingHits = -1;
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        public static string EncodeHits(int hits)
        {
            return Mathf.Clamp(hits, 1, MaxHits).ToString(); // a saved run is never dead
        }

        public static int DecodeHits(string json)
        {
            int value;
            if (string.IsNullOrEmpty(json) || !int.TryParse(json, out value)) return MaxHits;
            return Mathf.Clamp(value, 1, MaxHits);
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            if (data == null || Instance == null) return;
            SaveSystem.SetContrib(data, SaveKey, EncodeHits(Instance.HitsRemaining));
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            int hits = DecodeHits(SaveSystem.GetContrib(data, SaveKey));
            if (Instance != null) Instance.SetHits(hits);
            else pendingHits = hits;
        }

        void SetHits(int hits)
        {
            HitsRemaining = Mathf.Clamp(hits, 1, MaxHits);
            Action<float> changed = Changed;
            if (changed != null) changed(Normalized);
        }

        void Awake()
        {
            Instance = this;
            if (pendingHits >= 0)
            {
                int hits = pendingHits;
                pendingHits = -1;
                SetHits(hits);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Returns true while Nour is still standing after the strike.
        public bool ApplyHit()
        {
            if (HitsRemaining <= 0) return false;
            HitsRemaining--;
            LightShadow.Audio.Sfx.Play(HitsRemaining <= 0 ? "SFX_HurtHeavy" : "SFX_HurtHit");
            Action<float> changed = Changed;
            if (changed != null) changed(Normalized);
            if (HitsRemaining <= 0)
            {
                Action died = Died;
                if (died != null) died();
                return false;
            }
            Action hit = Hit;
            if (hit != null) hit();
            return true;
        }

        // Medkits are the only healing (user brief): one kit, back to full.
        public bool TryUseMedkit()
        {
            if (HitsRemaining >= MaxHits) return false;
            if (!Inventory.Has("medkit")) return false;
            Inventory.Remove("medkit");
            HitsRemaining = MaxHits;
            Action<float> changed = Changed;
            if (changed != null) changed(Normalized);
            return true;
        }

        public void ResetHealth()
        {
            if (HitsRemaining == MaxHits) return;
            HitsRemaining = MaxHits;
            Action<float> changed = Changed;
            if (changed != null) changed(Normalized);
        }

        // H belongs to hints (HintService and every puzzle UI), so the medkit
        // moved to M (audit: H/F key conflicts).
        public const KeyCode MedkitKey = KeyCode.M;

        void Update()
        {
            // Desktop hotkey; the mobile MEDKIT button calls TryUseMedkit
            // through TouchButton.Kind.Medkit.
            if (Input.GetKeyDown(MedkitKey))
            {
                if (TryUseMedkit())
                {
                    if (GameManager.Instance != null) GameManager.Instance.ShowMessage("Medkit used. Back on her feet.", 3f);
                }
                else if (GameManager.Instance != null && HitsRemaining < MaxHits)
                {
                    GameManager.Instance.ShowMessage("No medkit. They are rare - some puzzles hide one.", 3f);
                }
            }
        }
    }
}
