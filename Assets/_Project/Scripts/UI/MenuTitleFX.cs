using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    /// <summary>
    /// Horror flicker for the main-menu title block: the title and eyebrow
    /// occasionally dip in alpha like a failing fluorescent tube, then settle.
    /// Runs on unscaled time so it behaves identically in menus. The menu had
    /// zero animation before this — hand-rolled coroutine, no tween library.
    /// </summary>
    public class MenuTitleFX : MonoBehaviour
    {
        public Graphic[] flickerTargets;
        public float idleSecondsMin = 2.2f;
        public float idleSecondsMax = 6.5f;
        public float flickerSeconds = 0.55f;

        Color[][] savedColors;

        void Start()
        {
            int count = flickerTargets != null ? flickerTargets.Length : 0;
            savedColors = new Color[count][];
            for (int i = 0; i < count; i++)
            {
                if (flickerTargets[i] == null) continue;
                // Gradient-modified texts keep their gradient; store the base
                // color so each flicker tick can restore it exactly.
                savedColors[i] = new[] { flickerTargets[i].color };
            }
            StartCoroutine(FlickerLoop());
        }

        IEnumerator FlickerLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(Random.Range(idleSecondsMin, idleSecondsMax));
                float elapsed = 0f;
                while (elapsed < flickerSeconds)
                {
                    elapsed += Time.unscaledDeltaTime;
                    bool dark = Random.value < 0.5f;
                    ApplyScale(dark ? 0.35f : 1f);
                    yield return null;
                }
                ApplyScale(1f);
            }
        }

        void ApplyScale(float factor)
        {
            if (flickerTargets == null) return;
            for (int i = 0; i < flickerTargets.Length; i++)
            {
                if (flickerTargets[i] == null || savedColors == null || i >= savedColors.Length) continue;
                Color baseColor = savedColors[i][0];
                baseColor.a *= factor;
                flickerTargets[i].color = baseColor;
            }
        }
    }
}
