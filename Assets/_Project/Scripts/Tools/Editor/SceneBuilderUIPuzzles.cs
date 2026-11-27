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

        // Card-sort overlay (workstream 5), first used by the library card
        // catalogue: one clue card at a time, four drawer buttons.
        static void BuildCardSortPanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "CardSortPanel", new Color(0f, 0f, 0f, 0.82f), true);
            CardSortUIController ctrl = dim.gameObject.AddComponent<CardSortUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 700f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 274f), new Vector2(660f, 48f), FontStyle.Bold, true);

            Text counter = MakeText(card, "Counter", "", 19, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 232f), new Vector2(660f, 28f));

            // The index card the clue is written on.
            MakePanel(card, "IndexCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 138f), new Vector2(620f, 130f),
                new Color(0.13f, 0.12f, 0.10f, 0.95f), false);
            Text clue = MakeText(card, "Clue", "", 27, Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 138f), new Vector2(580f, 118f));

            Button[] drawers = new Button[4];
            Text[] drawerLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -172f : 172f;
                float y = i < 2 ? 8f : -72f;
                drawers[i] = MakeButton(card, "Drawer" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(324f, 66f), ButtonDark, 22, out drawerLabels[i]);
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -140f), new Vector2(660f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -188f), new Vector2(660f, 52f));

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -278f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -278f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "1-4 or tap a drawer   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(660f, 24f));
            Localize(footer, "1-4 أو المس درجًا   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.counterText = counter;
            ctrl.clueText = clue;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.drawerButtons = drawers;
            ctrl.drawerLabels = drawerLabels;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }

        // Optics-bench overlay (workstream 5), first used by the science lab:
        // three rotor buttons (90-degree steps), a beam diagram, a test button.
        static void BuildOpticsPanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "OpticsPanel", new Color(0f, 0f, 0f, 0.82f), true);
            OpticsUIController ctrl = dim.gameObject.AddComponent<OpticsUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 680f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 268f), new Vector2(640f, 48f), FontStyle.Bold, true);

            Text diagram = MakeText(card, "Diagram", "", 24, Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 196f), new Vector2(680f, 36f), FontStyle.Bold);

            Button[] rotors = new Button[3];
            Text[] rotorLabels = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float x = -220f + i * 220f;
                rotors[i] = MakeButton(card, "Rotor" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, 92f), new Vector2(200f, 76f), ButtonDark, 21, out rotorLabels[i]);
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 8f), new Vector2(640f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -46f), new Vector2(640f, 56f));

            Text testLabel;
            Button test = MakeButton(card, "Test", "TEST THE BEAM", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -132f), new Vector2(340f, 60f), ButtonWarm, 24, out testLabel);
            Localize(testLabel, "اختبر الشعاع");

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -232f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -232f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "1-3 rotate, Enter tests   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(640f, 24f));
            Localize(footer, "1-3 تدوير و Enter يختبر   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.diagramText = diagram;
            ctrl.rotorButtons = rotors;
            ctrl.rotorLabels = rotorLabels;
            ctrl.testButton = test;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }

        // Color-sequence overlay (workstream 5), first used by the stage light
        // rig: four colored swatches that flash the pattern, four switch
        // buttons to repeat it, a replay button.
        static void BuildSequencePanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "SequencePanel", new Color(0f, 0f, 0f, 0.82f), true);
            SequenceUIController ctrl = dim.gameObject.AddComponent<SequenceUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 700f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 274f), new Vector2(660f, 48f), FontStyle.Bold, true);

            Text progress = MakeText(card, "Progress", "", 19, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 232f), new Vector2(660f, 28f));

            // The rig's lamps: flat swatches the controller flashes. Colors
            // must be flat images, not themed panels, so the flash can paint.
            Color[] lampColors =
            {
                new Color(0.55f, 0.12f, 0.10f), new Color(0.65f, 0.42f, 0.10f),
                new Color(0.14f, 0.45f, 0.18f), new Color(0.12f, 0.25f, 0.60f),
            };
            Image[] swatches = new Image[4];
            Button[] switches = new Button[4];
            Text[] switchLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                float x = -246f + i * 164f;
                swatches[i] = MakeImage(card, "Lamp" + (i + 1), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, 150f), new Vector2(120f, 90f), BackgroundSprite,
                    lampColors[i], false);
                switches[i] = MakeButton(card, "Switch" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, 40f), new Vector2(140f, 66f), ButtonDark, 21, out switchLabels[i]);
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -38f), new Vector2(660f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -92f), new Vector2(660f, 52f));

            Text replayLabel;
            Button replay = MakeButton(card, "Replay", "REPLAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -178f), new Vector2(220f, 54f), ButtonWarm, 22, out replayLabel);
            Localize(replayLabel, "أعد العرض");

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -262f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -262f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "1-4 or tap a switch, R replays   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(660f, 24f));
            Localize(footer, "1-4 أو المس مفتاحًا و R يعيد   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.progressText = progress;
            ctrl.swatches = swatches;
            ctrl.switchButtons = switches;
            ctrl.switchLabels = switchLabels;
            ctrl.replayButton = replay;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }

        // Wiring-board overlay (workstream 5), used by the finale gate
        // console: three feeds on the left, three terminals on the right.
        static void BuildCircuitPanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "CircuitPanel", new Color(0f, 0f, 0f, 0.82f), true);
            CircuitUIController ctrl = dim.gameObject.AddComponent<CircuitUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 680f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 268f), new Vector2(660f, 48f), FontStyle.Bold, true);

            Text progress = MakeText(card, "Progress", "", 19, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 224f), new Vector2(660f, 28f));

            Button[] feeds = new Button[3];
            Text[] feedLabels = new Text[3];
            Button[] terminals = new Button[3];
            Text[] terminalLabels = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 148f - i * 88f;
                feeds[i] = MakeButton(card, "Feed" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(-182f, y), new Vector2(300f, 68f), ButtonDark, 21, out feedLabels[i]);
                terminals[i] = MakeButton(card, "Terminal" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(182f, y), new Vector2(300f, 68f), ButtonDark, 21, out terminalLabels[i]);
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -132f), new Vector2(660f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -184f), new Vector2(660f, 52f));

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -270f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -270f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "1-3 feed, Q/W/E terminal   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(660f, 24f));
            Localize(footer, "1-3 تغذية و Q/W/E طرف   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.progressText = progress;
            ctrl.feedButtons = feeds;
            ctrl.feedLabels = feedLabels;
            ctrl.terminalButtons = terminals;
            ctrl.terminalLabels = terminalLabels;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }

        // Step-ordering overlay (workstream 5), first used by the infirmary's
        // first-aid board: five scrambled step buttons, tap in the right order.
        static void BuildOrderPanel(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "OrderPanel", new Color(0f, 0f, 0f, 0.82f), true);
            OrderUIController ctrl = dim.gameObject.AddComponent<OrderUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 700f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "", 30, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 274f), new Vector2(660f, 48f), FontStyle.Bold, true);

            Text progress = MakeText(card, "Progress", "", 19, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 230f), new Vector2(660f, 28f));

            Button[] steps = new Button[5];
            Text[] stepLabels = new Text[5];
            for (int i = 0; i < 5; i++)
            {
                float y = 170f - i * 78f;
                steps[i] = MakeButton(card, "Step" + (i + 1), "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(600f, 62f), ButtonDark, 21, out stepLabels[i]);
            }

            Text feedback = MakeText(card, "Feedback", "", 23, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -152f), new Vector2(660f, 36f));

            Text hint = MakeText(card, "Hint", "", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -202f), new Vector2(660f, 52f));

            Text hintLabel;
            Button hintButton = MakeButton(card, "Hint", "HINT", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-196f, -286f), new Vector2(220f, 50f), ButtonDark, 20, out hintLabel);
            Localize(hintLabel, "تلميح");

            Text closeLabel;
            Button close = MakeButton(card, "Close", "STEP AWAY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(196f, -286f), new Vector2(220f, 50f), ButtonDark, 20, out closeLabel);
            Localize(closeLabel, "ابتعد");

            Text footer = MakeText(card, "Footer", "1-5 or tap a step   ·   the world does not pause", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(660f, 24f));
            Localize(footer, "1-5 أو المس خطوة   ·   العالم لا يتوقف");

            ctrl.panel = dim.gameObject;
            ctrl.titleText = title;
            ctrl.progressText = progress;
            ctrl.stepButtons = steps;
            ctrl.stepLabels = stepLabels;
            ctrl.feedbackText = feedback;
            ctrl.hintText = hint;
            ctrl.hintButton = hintButton;
            ctrl.closeButton = close;
        }
    }
}
