using System;
using System.Collections;
using System.Collections.Generic;
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
        public Transform hingeB;         // optional second leaf (double door); swings the mirrored way
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

        public const string SaveKey = "doors";

        [Serializable]
        public class DoorState
        {
            public string id = "";
            public bool open;
            public bool locked;
            public bool freed;
        }

        [Serializable]
        public class DoorsSave
        {
            public List<DoorState> doors = new List<DoorState>();
        }

        // Stable per-door identity: name plus rounded position (names repeat).
        public static string IdFor(string name, Vector3 position)
        {
            return name + "@" + Mathf.RoundToInt(position.x * 10f) + "," + Mathf.RoundToInt(position.y * 10f) + "," + Mathf.RoundToInt(position.z * 10f);
        }

        public string SaveId => IdFor(name, transform.position);

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
            if (data == null) return;
            DoorsSave save = new DoorsSave();
            DoorController[] all = FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                save.doors.Add(new DoorState { id = all[i].SaveId, open = all[i].IsOpen, locked = all[i].IsLocked, freed = all[i].PlayerFreed });
            }
            SaveSystem.SetContrib(data, SaveKey, JsonUtility.ToJson(save));
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            if (data == null) return;
            string json = SaveSystem.GetContrib(data, SaveKey);
            if (string.IsNullOrEmpty(json)) return;
            DoorsSave save = null;
            try { save = JsonUtility.FromJson<DoorsSave>(json); }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Door save blob unreadable, doors keep their start state: " + exception.Message);
            }
            if (save == null || save.doors == null) return;

            DoorController[] all = FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            HashSet<string> done = new HashSet<string>();
            for (int s = 0; s < save.doors.Count; s++)
            {
                DoorState state = save.doors[s];
                if (state == null || string.IsNullOrEmpty(state.id) || !done.Add(state.id)) continue; // each identity once
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].SaveId == state.id) { all[i].RestoreFromSave(state.open, state.locked, state.freed); break; }
                }
            }
        }

        // Instant restore: no swing, no sound. A lock only holds if the door
        // can lock at all (keyId set).
        public void RestoreFromSave(bool open, bool locked, bool freed)
        {
            if (swing != null) { StopCoroutine(swing); swing = null; }
            IsLocked = locked && !string.IsNullOrEmpty(keyId);
            IsOpen = open && !IsLocked;
            PlayerFreed = freed || IsOpen;
            if (hinge != null) hinge.localRotation = Quaternion.Euler(0f, IsOpen ? openAngle : 0f, 0f);
            if (hingeB != null) hingeB.localRotation = Quaternion.Euler(0f, IsOpen ? -openAngle : 0f, 0f);
            ApplyPassage();
        }

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
                if (IsLocked) return string.Format(Localization.Tr("Locked - {0} needed"), Inventory.DisplayName(keyId)); // logical text: the HUD shapes it once
                return IsOpen ? "Close the door" : "Open the door"; // source sentences: the HUD translates and shapes them once
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
                    LightShadow.Audio.Sfx.Play("SFX_DoorUnlock", ProceduralSfx.Power); // the lock clicks open
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowMessage(string.Format(Localization.Tr("Unlocked with the {0}."), Inventory.DisplayName(keyId)), 3f);
                    }
                }
                else
                {
                    LightShadow.Audio.Sfx.Play("SFX_DoorLockedRattle", ProceduralSfx.Wrong); // the locked rattle
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
            LightShadow.Audio.Sfx.Play(open ? "SFX_DoorOpen" : "SFX_DoorClose", ProceduralSfx.Click);
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
            Quaternion startB = hingeB != null ? hingeB.localRotation : Quaternion.identity;
            Quaternion targetB = Quaternion.Euler(0f, -targetAngle, 0f);
            float elapsed = 0f;
            while (elapsed < swingSeconds)
            {
                elapsed += Time.deltaTime;
                // Smoothstep so the swing eases in and out like a real hinge.
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, swingSeconds));
                t = t * t * (3f - 2f * t);
                hinge.localRotation = Quaternion.Slerp(start, target, t);
                if (hingeB != null) hingeB.localRotation = Quaternion.Slerp(startB, targetB, t);
                yield return null;
            }
            hinge.localRotation = target;
            if (hingeB != null) hingeB.localRotation = targetB;
            swing = null;
        }
    }
}
