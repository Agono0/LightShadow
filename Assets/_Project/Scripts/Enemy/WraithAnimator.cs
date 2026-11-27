using UnityEngine;

namespace LightShadow.Enemy
{
    // Plays the GhostSkull model's embedded clips from the EnemyBehaviour state.
    // Uses the legacy Animation component (no controller asset), and finds each
    // clip by name suffix so an importer prefix such as "CharacterArmature|"
    // does not matter. Not seen in the editor: clip names, scale and pivot are
    // unverified. Missing clips are skipped, never an error.
    public class WraithAnimator : MonoBehaviour
    {
        public EnemyBehaviour behaviour;
        public Animation animationPlayer;
        public AnimationClip[] clips;

        const float Fade = 0.15f;

        AnimationClip idle, fast, headbutt, hit;
        EnemyBehaviour.State lastState = (EnemyBehaviour.State)(-1);
        float ambushStartedAt;
        bool ready;

        void Awake()
        {
            if (animationPlayer == null || clips == null || clips.Length == 0) return;

            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null) continue;
                if (!clip.legacy) clip.legacy = true;
                animationPlayer.AddClip(clip, ShortName(clip.name));
            }

            idle = Find("Flying_Idle");
            fast = Find("Fast_Flying");
            headbutt = Find("Headbutt");
            hit = Find("HitReact");
            SetLoop(idle);
            SetLoop(fast);
            ready = idle != null || fast != null;
            if (ready) Play(idle != null ? idle : fast, true);
        }

        void Update()
        {
            if (!ready || behaviour == null) return;

            EnemyBehaviour.State state = behaviour.CurrentState;
            if (state != lastState)
            {
                lastState = state;
                if (state == EnemyBehaviour.State.Ambush) ambushStartedAt = Time.time;
                Play(ClipFor(state), false);
            }
            else if (state == EnemyBehaviour.State.Ambush && headbutt != null && fast != null
                     && Time.time - ambushStartedAt > fast.length)
            {
                // Burst first, then the headbutt once.
                if (!animationPlayer.IsPlaying(ShortName(headbutt.name))) Play(headbutt, false);
            }
        }

        AnimationClip ClipFor(EnemyBehaviour.State state)
        {
            switch (state)
            {
                case EnemyBehaviour.State.Stalk:
                case EnemyBehaviour.State.Investigate:
                case EnemyBehaviour.State.Chase:
                case EnemyBehaviour.State.Ambush:
                    return fast != null ? fast : idle;
                case EnemyBehaviour.State.Repelled:
                    return hit != null ? hit : idle;
                default:
                    return idle != null ? idle : fast;
            }
        }

        void Play(AnimationClip clip, bool instant)
        {
            if (clip == null) return;
            string name = ShortName(clip.name);
            if (instant) animationPlayer.Play(name);
            else animationPlayer.CrossFade(name, Fade);
        }

        AnimationClip Find(string suffix)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && clips[i].name.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
                    return clips[i];
            }
            return null;
        }

        static void SetLoop(AnimationClip clip)
        {
            if (clip != null) clip.wrapMode = WrapMode.Loop;
        }

        static string ShortName(string clipName)
        {
            int bar = clipName.LastIndexOf('|');
            return bar >= 0 ? clipName.Substring(bar + 1) : clipName;
        }
    }
}
