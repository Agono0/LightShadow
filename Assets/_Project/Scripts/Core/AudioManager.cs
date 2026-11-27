using UnityEngine;

namespace LightShadow.Core
{
    // Thin, null-safe wrapper around three AudioSources: music, ambience and SFX.
    // Every gameplay system talks to sound through here so missing clips can
    // never crash a scene that is still being built.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Wiring")]
        public AudioSource musicSource;
        public AudioSource ambienceSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;

        void Awake()
        {
            Instance = this;
            // Reverb zone mix policy (Docs/REVERB_ZONES.md): voice stays dry
            // so narration reads as inner voice in every room. Music and SFX
            // keep the default mix (1) and take the room's reverb; UI clicks
            // share sfxSource until a dedicated UI source exists.
            if (voiceSource != null) voiceSource.reverbZoneMix = 0f;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // One-shot music (the memory-flash piano): plays without looping and
        // without the already-playing guard, so a flash always restarts the
        // theme. AdaptiveMusic.UpdateBed re-asserts the looped bed afterwards.
        public void PlayMusicOnce(AudioClip clip)
        {
            if (clip == null || musicSource == null) return;
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void PlayAmbience(AudioClip clip)
        {
            if (clip == null || ambienceSource == null) return;
            if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }

        // Story voiceover: one line at a time, interrupting itself, never
        // looping. Generated clips play through here (VoiceoverPlayer) so a
        // new node line always cuts the previous one off.
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null || voiceSource == null) return;
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        public void StopVoice()
        {
            if (voiceSource != null) voiceSource.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip);
        }

        // Volume-scaled one-shot (P3 calm FX heartbeat fades with intensity).
        public void PlaySfx(AudioClip clip, float volume)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        // Positional one-shot through a pooled child source instead of
        // AudioSource.PlayClipAtPoint: PlayClipAtPoint creates a temp
        // "OneShot Audio" source that bypasses VolumeCategories and any mixer
        // routing (D15 follow-up §4.5). Named "SfxOneShot" so the scan maps it
        // to Sfx; explicitly registered/routed at creation too.
        AudioSource oneShotSource;

        public void PlaySfxAt(AudioClip clip, Vector3 position)
        {
            PlaySfxAt(clip, position, 1f);
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null) return;
            AudioSource source = GetOneShotSource();
            if (source == null) return;
            source.transform.position = position;
            source.spatialBlend = 1f;
            // Positional sounds fade out with distance instead of carrying
            // across the whole school (default max distance is 500 m).
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1.5f;
            source.maxDistance = LightShadow.Audio.DangerModel.AudibleRadius(volume);
            source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        AudioSource GetOneShotSource()
        {
            if (oneShotSource != null) return oneShotSource;

            var go = new GameObject("SfxOneShot");
            go.transform.SetParent(transform, false);
            oneShotSource = go.AddComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
            LightShadow.Audio.VolumeCategories.Register(
                oneShotSource, LightShadow.Audio.VolumeCategories.Category.Sfx, 1f);
            return oneShotSource;
        }
    }
}
