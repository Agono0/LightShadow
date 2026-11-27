using UnityEngine;

namespace LightShadow.Player
{
    // Dust motes in the torch beam (user brief 2026-11-27: subtle, tiny, on
    // mobile and PC alike). The builder shapes the ParticleSystem; this only
    // keeps the motes in step with the beam so a dead or switched-off torch
    // shows no floating dust.
    public class BeamDust : MonoBehaviour
    {
        public Light beam;
        public ParticleSystem motes;

        void Update()
        {
            if (motes == null) return;
            bool lit = beam != null && beam.enabled && beam.gameObject.activeInHierarchy;
            ParticleSystem.EmissionModule emission = motes.emission;
            if (emission.enabled != lit) emission.enabled = lit;
        }
    }
}
