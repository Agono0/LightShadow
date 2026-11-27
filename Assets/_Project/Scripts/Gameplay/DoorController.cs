using System.Collections;
using LightShadow.Core;
using LightShadow.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace LightShadow.Gameplay
{
    // Interactive door (user brief 2026-11-27): open / close / locked with a
    // key from the inventory. The wraith's passage rule is the user's own: it
    // can come through a door only after Nour has freed it herself. While she
    // has not, a carving NavMeshObstacle keeps the doorway out of its paths;
    // once she has, the obstacle lifts and a closed-but-freed door swings for
    // the wraith as it arrives. Doors sit on the Default layer, so the
    // LevelLayer-only NavMesh bake never seals the doorway permanently.
    public class DoorController : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public Transform hinge;          // rotates on local Y to swing the panel
        public string keyId = "";        // empty = never locks
        public bool startsLocked;

        [Header("Tuning")]
        public float openAngle = -105f;  // sign picks the swing side
        public float swingSeconds = 0.55f;

        public bool IsOpen { get; private set; }
        public bool IsLocked { get; private set; }

        // First time Nour opens or unlocks the door. The wraith's rule keys
        // off this and nothing else.
        public bool PlayerFreed { get; private set; }

        NavMeshObstacle obstacle;
        Coroutine swing;

        void Awake()
        {
            IsLocked = startsLocked && !string.IsNullOrEmpty(keyId);
            obstacle = GetComponent<NavMeshObstacle>();
            ApplyPassage();
        }

        public string Prompt
        {
            get
            {
                if (IsLocked) return "Locked - " + Inventory.DisplayName(keyId) + " needed";
                return IsOpen ? "Close the door" : "Open the door";
            }
        }

        public bool CanInteract => true;

        public void Interact(GameObject interactor)
        {
            if (IsLocked)
            {
                if (!string.IsNullOrEmpty(keyId) && Inventory.Has(keyId))
                {
                    IsLocked = false;
                    PlayerFreed = true;
                    ApplyPassage();
                    Play(ProceduralSfx.Power); // the lock clicks open
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowMessage("Unlocked with the " + Inventory.DisplayName(keyId) + ".", 3f);
                    }
                }
                else
                {
                    Play(ProceduralSfx.Wrong); // the locked rattle
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowMessage("Locked. The key must be somewhere in this school.", 3f);
                    }
                }
                return;
            }

            if (!IsOpen) PlayerFreed = true;
            SetOpen(!IsOpen);
            ApplyPassage();
        }

        public void SetOpen(bool open)
        {
            if (IsOpen == open) return;
            IsOpen = open;
            if (swing != null) StopCoroutine(swing);
            swing = StartCoroutine(Swing(open ? openAngle : 0f));
            Play(ProceduralSfx.Click); // placeholder creak until bro's door audio ships
        }

        void Update()
        {
            // A freed door swings for the wraith as it arrives; a door Nour
            // never freed stays out of its paths entirely (the obstacle).
            if (IsLocked || IsOpen || !PlayerFreed) return;
            EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            if (enemy == null) return;
            Vector3 a = enemy.transform.position;
            Vector3 b = transform.position;
            a.y = 0f;
            b.y = 0f;
            if ((a - b).sqrMagnitude <= 1.8f * 1.8f)
            {
                SetOpen(true);
                ApplyPassage();
            }
        }

        void ApplyPassage()
        {
            // Carve the doorway out of the wraith's NavMesh only while the
            // panel is shut and Nour has never freed it.
            if (obstacle != null) obstacle.enabled = !IsOpen && !PlayerFreed;
        }

        IEnumerator Swing(float targetAngle)
        {
            if (hinge == null) yield break;
            Quaternion start = hinge.localRotation;
            Quaternion target = Quaternion.Euler(0f, targetAngle, 0f);
            float elapsed = 0f;
            while (elapsed < swingSeconds)
            {
                elapsed += Time.deltaTime;
                // Smoothstep so the swing eases in and out like a real hinge.
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, swingSeconds));
                t = t * t * (3f - 2f * t);
                hinge.localRotation = Quaternion.Slerp(start, target, t);
                yield return null;
            }
            hinge.localRotation = target;
            swing = null;
        }

        static void Play(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip);
        }
    }
}
