using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tools
{
    public static partial class SceneBuilderUI
    {
        // In-game credits (Docs/TASKS.md): a scrolling panel on the main menu
        // rendered from Docs/CREDITS.md at scene-build time (the file is
        // generated from the asset license registry). Markdown is reduced to
        // rich text: headings bold, URLs dropped, bullets as dots. English only.
        static string CreditsRichText()
        {
            const string path = "Docs/CREDITS.md";
            if (!File.Exists(path)) return "Credits: see Docs/CREDITS.md";

            var sb = new StringBuilder();
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.TrimEnd();
                if (line.StartsWith("# ")) { sb.AppendLine("<b><size=30>" + line.Substring(2) + "</size></b>"); continue; }
                if (line.StartsWith("## ")) { sb.AppendLine("\n<b>" + line.Substring(3) + "</b>"); continue; }
                line = Regex.Replace(line, @"\s*https?://\S+", "");
                line = Regex.Replace(line, @"\*\*(.+?)\*\*", "<b>$1</b>");
                line = line.Replace("*", "").Replace("`", "");
                if (line.StartsWith("- ")) line = "\u2022 " + line.Substring(2);
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        static GameObject BuildCreditsPanel(Transform root, MainMenuController controller)
        {
            RectTransform dim = MakeFullScreen(root, "CreditsPanel", new Color(0f, 0f, 0f, 0.9f), true);
            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 760f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "CREDITS", 42, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -44f), new Vector2(880f, 56f), FontStyle.Bold, true);
            Localize(title);

            Text body;
            CreateScroll(card, new Vector2(40f, -96f), new Vector2(920f, 540f), out body);
            body.text = CreditsRichText();

            Text closeLabel;
            Button close = MakeButton(card, "CloseButton", "BACK", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(300f, 56f),
                ButtonWarm, 21, out closeLabel);
            Localize(closeLabel);
            controller.closeCreditsButton = close;

            dim.gameObject.SetActive(false);
            return dim.gameObject;
        }
    }
}
