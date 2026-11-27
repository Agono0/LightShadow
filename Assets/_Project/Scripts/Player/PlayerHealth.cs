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

        void Awake()
        {
            Instance = this;
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
