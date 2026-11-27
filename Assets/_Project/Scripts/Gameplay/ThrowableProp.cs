using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Carry/throw distraction props (user brief 2026-11-27): E picks a prop up,
    // E sets it down, F (or the THROW touch button) hurls it. Where it lands
    // the wraith HEARS it - the noise is positional (EnemyBehaviour.HearNoiseAt),
    // not the player's position - so a thrown bottle genuinely pulls the hunt
    // away from Nour. Breakable props shatter on a hard landing with a louder
    // noise and a few fading shards.
    public class ThrowableProp : MonoBehaviour, IInteractable
    {
        [Header("Tuning")]
        public bool breakable = true;
        public float noiseRadius = 14f;
        public float breakNoiseRadius = 20f;
        public float throwForce = 6.5f;

        public static ThrowableProp Carried { get; private set; }

        Rigidbody body;
        Vector3 homePosition;
        Quaternion homeRotation;
        Transform homeParent;
        bool landedSinceRelease = true;

        public string Prompt => Carried == this ? "Put it down" : "Pick it up";
        public bool CanInteract => Carried == null || Carried == this;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeParent = transform.parent;
        }

        public void Interact(GameObject interactor)
        {
            if (Carried == this) Drop();
            else if (Carried == null) PickUp();
        }

        void PickUp()
        {
            Carried = this;
            if (body != null) body.isKinematic = true;
            Transform hold = HoldPoint();
            transform.SetParent(hold, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Play(ProceduralSfx.Pickup);
        }

        void Drop()
        {
            Release(Vector3.zero);
        }

        // ThrowCarried backs the F key and the THROW touch button.
        public static void ThrowCarried()
        {
            if (Carried == null) return;
            Camera cam = Camera.main;
            Vector3 impulse = cam != null
                ? (cam.transform.forward + Vector3.up * 0.15f).normalized * Carried.throwForce
                : Vector3.forward * Carried.throwForce;
            Carried.Release(impulse);
        }

        void Release(Vector3 impulse)
        {
            Carried = null;
            transform.SetParent(homeParent, true);
            if (body != null)
            {
                body.isKinematic = false;
                body.velocity = impulse;
                body.angularVelocity = Random.insideUnitSphere * 3f;
            }
            landedSinceRelease = false;
            if (impulse != Vector3.zero) LightShadow.Audio.Sfx.PlayAt("SFX_ThrowWhoosh", transform.position);
        }

        void Update()
        {
            if (Carried == this && !PlayerController.ExternalInputLocked)
            {
                if (Input.GetKeyDown(KeyCode.F)) ThrowCarried();
                // E while carrying means "put it down"; the InteractionRaycaster
                // defers to us while Carried is set.
                else if (Input.GetKeyDown(KeyCode.E)) Drop();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (landedSinceRelease) return;
            if (collision.relativeVelocity.magnitude < 1.5f) return;
            landedSinceRelease = true;

            // The landing clatter is what pulls the wraith - at the prop, not
            // at Nour.
            Enemy.EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            bool shatters = breakable && collision.relativeVelocity.magnitude > 3.5f;
            if (enemy != null) enemy.HearNoiseAt(transform.position, shatters ? breakNoiseRadius : noiseRadius);

            if (shatters)
            {
                LightShadow.Audio.Sfx.PlayAt("SFX_GlassCrack", transform.position, ProceduralSfx.Power);
                LightShadow.Audio.Sfx.PlayAt("SFX_GlassShard", transform.position);
                SpawnShards();
                gameObject.SetActive(false);
            }
            else
            {
                LightShadow.Audio.Sfx.PlayAt(collision.relativeVelocity.magnitude > 4f ? "SFX_ThrowImpactHard" : "SFX_ThrowImpactSoft", transform.position, ProceduralSfx.Click);
            }
        }

        void SpawnShards()
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = name + "_Shard";
                shard.transform.position = transform.position + Random.insideUnitSphere * 0.08f;
                shard.transform.localScale = Vector3.one * 0.05f;
                Renderer renderer = shard.GetComponent<Renderer>();
                Renderer source = GetComponentInChildren<Renderer>();
                if (renderer != null && source != null) renderer.sharedMaterial = source.sharedMaterial;
                Rigidbody shardBody = shard.AddComponent<Rigidbody>();
                shardBody.mass = 0.05f;
                shardBody.velocity = Random.insideUnitSphere * 1.6f + Vector3.up * 1.2f;
                Destroy(shard, 3f);
            }
        }

        // Back in place on a checkpoint wake: a carried or shattered prop
        // resets with the chapter.
        public void ResetHome()
        {
            if (Carried == this) Carried = null;
            gameObject.SetActive(true);
            transform.SetParent(homeParent, false);
            transform.position = homePosition;
            transform.rotation = homeRotation;
            if (body != null)
            {
                body.isKinematic = false;
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            landedSinceRelease = true;
        }

        static Transform HoldPoint()
        {
            Camera cam = Camera.main;
            if (cam == null) return null;
            Transform hold = cam.transform.Find("CarryHold");
            if (hold == null)
            {
                var go = new GameObject("CarryHold");
                hold = go.transform;
                hold.SetParent(cam.transform, false);
                hold.localPosition = new Vector3(0.25f, -0.22f, 0.55f);
            }
            return hold;
        }

        static void Play(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip);
        }
    }
}
