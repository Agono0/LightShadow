using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    /// <summary>
    /// Generates the modern UI sprite set (player request: menus looked dated —
    /// flat rects with Unity's built-in knob sprite). Everything is drawn
    /// procedurally into Assets/_Project/Textures/UI/, so the look stays
    /// license-free and reproducible:
    ///
    ///   UI_Panel    9-slice rounded card: vertical gradient fill, soft border.
    ///   UI_Button   9-slice rounded button: top-lit gradient + highlight line.
    ///   UI_Glow     radial falloff used behind the title and key moments.
    ///   UI_Backdrop full-HD menu backdrop: night gradient, moon glow, the
    ///               school's rooftop silhouette with a few lit windows, ground
    ///               fog, vignette and film grain.
    ///
    /// Generation is deterministic (fixed seed) and idempotent: each run
    /// rewrites the same pixels. Batch entry: UiThemeTool.GenerateBatch —
    /// called from SceneBuilder.BuildAll before any canvas is created.
    /// </summary>
    public static class UiThemeTool
    {
        const string Folder = "Assets/_Project/Textures/UI";
        const int Border = 32; // 9-slice border baked into panel/button sprites

        public static void GenerateBatch()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        [MenuItem("LightShadow/Art/4. Generate UI Theme", priority = 24)]
        public static void GenerateMenu()
        {
            Generate();
            Debug.Log("[LightShadow] UI theme sprites generated.");
        }

        public static void Generate()
        {
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
            Write("UI_Panel.png", PanelTexture(), 256);
            Write("UI_Button.png", ButtonTexture(), 256);
            Write("UI_Glow.png", GlowTexture(), 256);
            Write("UI_Backdrop.png", BackdropTexture(), 960);
            AssetDatabase.Refresh();
        }

        static void Write(string fileName, Color[] pixels, int width)
        {
            var texture = new Texture2D(width, pixels.Length / width, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(Folder + "/" + fileName, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            const string path = Folder + "/"; // keep the import block readable
            string assetPath = path + fileName;
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (fileName.StartsWith("UI_Panel") || fileName.StartsWith("UI_Button"))
            {
                importer.spriteBorder = new Vector4(Border, Border, Border, Border);
            }
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------ panel --

        static Color[] PanelTexture()
        {
            int size = 256;
            var pixels = new Color[size * size];
            var fillTop = new Color(0.086f, 0.094f, 0.118f, 0.96f);
            var fillBottom = new Color(0.043f, 0.047f, 0.063f, 0.96f);
            var border = new Color(0.42f, 0.40f, 0.34f, 0.55f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = RoundedRectDistance(x, y, size, size, 30f);
                    Color pixel;
                    if (distance > 1f)
                    {
                        pixel = Color.clear;
                    }
                    else if (distance > 1f - 2.5f / 30f)
                    {
                        // 2-3 px edge band: the visible rounded border.
                        pixel = border;
                    }
                    else
                    {
                        float gradient = 1f - (float)y / size;
                        pixel = Color.Lerp(fillTop, fillBottom, gradient);
                    }
                    pixels[y * size + x] = pixel;
                }
            }
            return pixels;
        }

        // ------------------------------------------------------------ button --

        static Color[] ButtonTexture()
        {
            int width = 256, height = 128;
            var pixels = new Color[width * height];
            // Moonlit-night retheme (user brief 2026-11-27): slate fills and a
            // moon-silver border replace the old muddy browns.
            var fillTop = new Color(0.16f, 0.19f, 0.25f, 0.98f);
            var fillBottom = new Color(0.09f, 0.11f, 0.15f, 0.98f);
            var border = new Color(0.52f, 0.60f, 0.72f, 0.70f);
            var highlight = new Color(0.85f, 0.90f, 1f, 0.08f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float distance = RoundedRectDistance(x, y, width, height, 24f);
                    Color pixel;
                    if (distance > 1f)
                    {
                        pixel = Color.clear;
                    }
                    else if (distance > 1f - 2.5f / 24f)
                    {
                        pixel = border;
                    }
                    else
                    {
                        float gradient = 1f - (float)y / height;
                        pixel = Color.Lerp(fillTop, fillBottom, gradient);
                        // A soft lit band along the top third reads as depth.
                        float band = Mathf.Clamp01(1f - y / (height * 0.34f));
                        pixel = Color.Lerp(pixel, highlight, band * 0.5f);
                    }
                    pixels[y * width + x] = pixel;
                }
            }
            return pixels;
        }

        // -------------------------------------------------------------- glow --

        static Color[] GlowTexture()
        {
            int size = 256;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size - 0.5f;
                    float dy = (y + 0.5f) / size - 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float alpha = Mathf.Clamp01(1f - distance);
                    alpha *= alpha; // quadratic falloff keeps the centre soft
                    pixels[y * size + x] = new Color(1f, 0.93f, 0.78f, alpha);
                }
            }
            return pixels;
        }

        // ----------------------------------------------------------- backdrop --

        // 1920×1080 menu backdrop: night gradient, moon glow, rooftop skyline
        // with sparse lit windows, ground fog, vignette and film grain. The
        // silhouette is drawn as filled shapes so it stays sharp and cheap.
        static Color[] BackdropTexture()
        {
            int width = 960, height = 540; // generated at half res, imported crisp enough for UI
            var pixels = new Color[width * height];
            var random = new System.Random(1947); // fixed seed: deterministic grain/windows
            var skyline = BuildSkyline(width, height, random);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;

                    // Night gradient, slightly lifted near the horizon.
                    Color sky = Color.Lerp(new Color(0.043f, 0.055f, 0.086f), new Color(0.016f, 0.020f, 0.034f), v);

                    // Moon glow upper right.
                    float moonDx = u - 0.76f, moonDy = v - 0.80f;
                    float moon = Mathf.Clamp01(1f - Mathf.Sqrt(moonDx * moonDx + moonDy * moonDy) * 3.4f);
                    sky += new Color(0.55f, 0.50f, 0.38f, 0f) * (moon * moon * 0.55f);

                    // Ground fog band just above the skyline base.
                    float fog = Mathf.Clamp01(1f - Mathf.Abs(v - 0.815f) * 14f);
                    sky += new Color(0.10f, 0.13f, 0.19f, 0f) * (fog * 0.5f);

                    // Vignette.
                    float edge = Mathf.Clamp01((Mathf.Abs(u - 0.5f) * 1.35f) + (Mathf.Abs(v - 0.5f) * 1.15f));
                    sky *= 1f - Mathf.Clamp01(edge - 0.55f) * 1.1f;

                    // Film grain (±3%).
                    float grain = (float)(random.NextDouble() - 0.5) * 0.06f;
                    sky += new Color(grain, grain, grain, 0f);

                    // Rooftop silhouette + lit windows on top of the sky.
                    if (skyline[y * width + x].a > 0f)
                    {
                        Color building = new Color(0.014f, 0.018f, 0.030f, 1f);
                        Color window = skyline[y * width + x];
                        sky = Color.Lerp(sky, building, 1f);
                        if (window.r > 0.5f) sky = new Color(0.98f, 0.78f, 0.42f);
                    }

                    pixels[y * width + x] = sky;
                }
            }
            return pixels;
        }

        // Fills a height map of the school silhouette, then stamps a handful of
        // lit windows. Alpha marks "building", red channel marks "lit window".
        static Color[] BuildSkyline(int width, int height, System.Random random)
        {
            var mask = new Color[width * height];
            int baseY = (int)(height * 0.82f);

            // Main block, classroom wing, tower, gym annex — school-ish profile.
            StampRect(mask, width, height, 0.06f, 0.34f, 0.82f, 1.00f);
            StampRect(mask, width, height, 0.36f, 0.55f, 0.78f, 1.00f);
            StampRect(mask, width, height, 0.50f, 0.56f, 0.62f, 1.00f); // clock tower
            StampRect(mask, width, height, 0.58f, 0.86f, 0.84f, 1.00f);
            StampRect(mask, width, height, 0.88f, 1.00f, 0.86f, 1.00f);

            // Pitched roofs on two blocks.
            StampTriangle(mask, width, height, 0.06f, 0.20f, 0.82f, 0.76f);
            StampTriangle(mask, width, height, 0.58f, 0.72f, 0.84f, 0.78f);

            // Sparse lit windows (kept dim — the school is nearly empty tonight).
            int litWindows = 14;
            for (int i = 0; i < litWindows; i++)
            {
                int wx = random.Next((int)(width * 0.08f), (int)(width * 0.94f));
                int wy = baseY + random.Next(-10, 60);
                if (wy < 0 || wy >= height || mask[wy * width + wx].a <= 0f) continue;
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int px = Mathf.Min(width - 1, wx + dx);
                        int py = Mathf.Min(height - 1, wy + dy);
                        mask[py * width + px] = new Color(1f, 0f, 0f, 1f);
                    }
                }
            }
            return mask;
        }

        static void StampRect(Color[] mask, int width, int height, float x0, float x1, float yTop, float yBottom)
        {
            int px0 = Mathf.Clamp((int)(x0 * width), 0, width - 1);
            int px1 = Mathf.Clamp((int)(x1 * width), 0, width - 1);
            int py0 = Mathf.Clamp((int)((1f - yTop) * height), 0, height - 1);
            int py1 = Mathf.Clamp((int)((1f - yBottom) * height), 0, height - 1);
            for (int y = py0; y <= py1; y++)
            {
                for (int x = px0; x <= px1; x++)
                {
                    mask[y * width + x] = new Color(0f, 0f, 0f, 1f);
                }
            }
        }

        static void StampTriangle(Color[] mask, int width, int height, float x0, float x1, float yApex, float yBase)
        {
            int px0 = (int)(x0 * width);
            int px1 = (int)(x1 * width);
            int pyApex = (int)((1f - yApex) * height);
            int pyBase = (int)((1f - yBase) * height);
            for (int x = px0; x <= px1; x++)
            {
                float t = (float)(x - px0) / Mathf.Max(1, px1 - px0);
                float rise = Mathf.Lerp(pyApex, pyBase, Mathf.Abs(t - 0.5f) * 2f);
                for (int y = Mathf.Max(0, (int)rise); y <= Mathf.Min(height - 1, pyBase); y++)
                {
                    mask[y * width + x] = new Color(0f, 0f, 0f, 1f);
                }
            }
        }

        // ------------------------------------------------------------- shared --

        // Signed-distance-ish measure for a rounded rectangle: 0 at the centre,
        // 1 at the edge, >1 outside. radiusPx controls the corner rounding.
        static float RoundedRectDistance(int x, int y, int width, int height, float radiusPx)
        {
            float cx = (width - 1) * 0.5f;
            float cy = (height - 1) * 0.5f;
            float dx = Mathf.Abs(x - cx) - (cx - radiusPx);
            float dy = Mathf.Abs(y - cy) - (cy - radiusPx);
            float outsideX = Mathf.Max(dx, 0f);
            float outsideY = Mathf.Max(dy, 0f);
            float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
            float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
            float distance = (outside + inside) / radiusPx; // negative inside, 0..1 near edge
            return 1f + distance;                           // 0..1 inside band, >1 outside
        }
    }
}
