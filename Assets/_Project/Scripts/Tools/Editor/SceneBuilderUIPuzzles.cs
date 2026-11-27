using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tools
{
    // Wing puzzle overlays (finish-the-game plan, workstream 5): the digit-lock
    // panel the classroom cipher and later code puzzles share. Built in code
    // like the rest of the interface; the controller self-registers its static
    // Instance at runtime, so no GameManager wiring is needed.
    public static partial class SceneBuilderUI
    {
        static void BuildCodeLockPanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "CodeLockPanel", new Color(0f, 0f, 0f, 0.82f), true);
            CodeLockUIController ctrl = dim.gameObject.AddComponent<CodeLockUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 680f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 268f), new Vector2(640f, 48f), FontStyle.Bold, true);

            Text sequence = MakeText(card, "Sequence", "", 36, Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 148f), new Vector2(640f, 170f));

            Text[] digits = new Text[4];
            Button[] plus = new Button[4];
            Button[] minus = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float x = -198f + i * 132f;
                RectTransform wheel = MakePanel(card, "Digit" + (i + 1), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, 6f), new Vector2(112f, 176f), PanelDark, false).rectTransform;

                digits[i] = MakeText(wheel, "Num", "0", 58, Paper, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);

                Text plusLabel;
                plus[i] = MakeButton(wheel, "Plus", "+", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(84f, 40f), ButtonDark, 26, out plusLabel);
                plusLabel.raycastTarget = false;

                Text minusLabel;
                minus[i] = MakeButton(wheel, "Minus", "-", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(84f, 40f), ButtonDark, 26, out minusLabel);
                minusLabel.raycastTarget = false;
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -104f), new Vector2(640f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -154f), new Vector2(640f, 56f));

            Text confirmLabel;
            Button confirm = MakeButton(card, "Confirm", "TRY THE LOCK", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -222f), new Vector2(340f, 60f), ButtonWarm, 24, out confirmLabel);
            Localize(confirmLabel, "جرّب القفل");

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -296f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -296f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "0-9 then Enter   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(640f, 24f));
            Localize(footer, "الأرقام ثم Enter   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.sequenceText = sequence;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.digitTexts = digits;
            ctrl.plusButtons = plus;
            ctrl.minusButtons = minus;
            ctrl.confirmButton = confirm;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }
    }
}
