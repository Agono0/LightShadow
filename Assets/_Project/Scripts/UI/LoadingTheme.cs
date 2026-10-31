using UnityEngine;

namespace LightShadow.UI
{
    /// <summary>
    /// Runtime-generated textures for the loading screen's redesigned look —
    /// the loading overlay self-creates outside any scene, so it cannot load
    /// theme sprites from assets. Three tiny textures, generated once and
    /// cached: a night gradient backdrop, a soft radial glow (the sweeping
    /// flashlight), and a rounded 9-slice panel for the progress track.
    /// </summary>
    public static class LoadingTheme
    {
        static Texture2D gradient;
        static Texture2D glow;
        static Texture2D panel;

        public static Texture2D Gradient
        {
            get
            {
                if (gradient == null) gradient = BuildGradient();
                return gradient;
            }
        }

        public static Texture2D Glow
        {
            get
            {
                if (glow == null) glow = BuildGlow();
                return glow;
            }
        }

        public static Texture2D Panel
        {
            get
            {
                if (panel == null) panel = BuildPanel();
                return panel;
            }
        }

        static Texture2D BuildGradient()
        {
            var texture = new Texture2D(16, 256, TextureFormat.RGBA32, false);
            for (int y = 0; y < 256; y++)
            {
                float t = y / 255f;
                Color top = new Color(0.010f, 0.013f, 0.024f);
                Color bottom = new Color(0.035f, 0.040f, 0.062f);
                Color pixel = Color.Lerp(bottom, top, t);
                for (int x = 0; x < 16; x++) texture.SetPixel(x, y, pixel);
            }
            texture.Apply();
            return texture;
        }

        static Texture2D BuildGlow()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float falloff = Mathf.Clamp01(1f - distance);
                    byte alpha = (byte)(falloff * falloff * falloff * 255f);
                    texture.SetPixel(x, y, new Color32(255, 240, 205, alpha));
                }
            }
            texture.Apply();
            return texture;
        }

        static Texture2D BuildPanel()
        {
            const int size = 64;
            const float radius = 14f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size / 2f) - (size / 2f - radius), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size / 2f) - (size / 2f - radius), 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = distance < radius ? (byte)235 : (byte)0;
                    // Slight vertical sheen so the track reads as brushed metal.
                    byte shade = (byte)(70 + (y * 20 / size));
                    texture.SetPixel(x, y, new Color32(shade, shade, (byte)(shade + 10), alpha));
                }
            }
            texture.Apply();
            return texture;
        }

        /// <summary>Sprite helper for the generated textures (9-slice for the panel).</summary>
        public static Sprite Sprite(Texture2D texture, string name, Vector4 border)
        {
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprite.name = name;
            return sprite;
        }
    }
}
