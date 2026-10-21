using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LightShadow.Tools
{
    // Programmatic uGUI for the three UI scenes. Keeping the UI in code means the
    // whole interface is rebuilt identically on every machine - no prefab drift,
    // no missing font references, no hand-wiring mistakes.
    public static class SceneBuilderUI
    {
        static Font _font;
        static Sprite _background;
        static Sprite _knob;
        static Sprite _ui;

        public static Font UIFont
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null) _font = UnityEngine.Font.CreateDynamicFontFromOSFont("Arial", 16);
                return _font;
            }
        }

        static Sprite BackgroundSprite
        {
            get
            {
                if (_background == null) _background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                return _background;
            }
        }

        static Sprite KnobSprite
        {
            get
            {
                if (_knob == null) _knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                return _knob;
            }
        }

        static Sprite UISprite
        {
            get
            {
                if (_ui == null) _ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                return _ui;
            }
        }

        static readonly Color Warm = new Color(1f, 0.86f, 0.62f);
        static readonly Color Paper = new Color(0.90f, 0.88f, 0.80f);
        static readonly Color Dim = new Color(0.55f, 0.55f, 0.58f);
        static readonly Color PanelDark = new Color(0.055f, 0.055f, 0.065f, 0.97f);
        static readonly Color ButtonDark = new Color(0.16f, 0.15f, 0.14f, 0.95f);
        static readonly Color ButtonWarm = new Color(0.36f, 0.25f, 0.13f, 0.98f);

        public sealed class SchoolUI
        {
            public Canvas canvas;
            public HUDController hud;
            public QuizUIController quiz;
            public PauseMenu pause;
            public TouchInput touch;
            public GameObject mobileRoot;
        }

        // ============================================================= bootstrap

        public static Canvas BuildBootstrapUI()
        {
            Canvas canvas = CreateCanvas("UI_Bootstrap");
            Transform root = canvas.transform;

            MakeFullScreen(root, "Background", new Color(0.01f, 0.01f, 0.014f, 1f), false);
            MakeImage(root, "Glow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 600f), KnobSprite,
                new Color(1f, 0.72f, 0.36f, 0.07f), false);

            Text title = MakeText(root, "Title", "LIGHT SHADOW", 64, new Color(0.95f, 0.89f, 0.77f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 80f), new Vector2(1200f, 90f), FontStyle.Bold, true);
            Localize(title);

            Text loading = MakeText(root, "Loading", "A game for good  ·  no combat, no gore", 19,
                new Color(0.55f, 0.53f, 0.49f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f), new Vector2(800f, 28f));
            Localize(loading);

            return canvas;
        }

        // ================================================================ school

        public static SchoolUI BuildSchoolUI(Transform player)
        {
            Canvas canvas = CreateCanvas("UI_School");
            Transform root = canvas.transform;

            HUDController hud = canvas.gameObject.AddComponent<HUDController>();
            PauseMenu pause = canvas.gameObject.AddComponent<PauseMenu>();

            // ---- gauges -----------------------------------------------------
            Text meterLabel;
            Image meterFill = MakeBar(root, "MeterBar", new Vector2(36f, -26f), new Vector2(340f, 20f),
                Warm, "BRIGHT IDEA", out meterLabel);

            Text batteryLabel;
            Image batteryFill = MakeBar(root, "BatteryBar", new Vector2(36f, -88f), new Vector2(250f, 12f),
                new Color(0.55f, 0.82f, 0.90f), "BATTERY", out batteryLabel);

            // ---- prompt, message, crosshair ---------------------------------
            Text prompt = MakeText(root, "PromptText", "", 23, Paper, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 64f), new Vector2(1100f, 44f));

            Text message = MakeText(root, "MessageText", "", 28, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 170f), new Vector2(1180f, 180f), FontStyle.Italic, true);

            Image crosshair = MakeImage(root, "Crosshair", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 8f), KnobSprite,
                new Color(1f, 1f, 1f, 0.45f), false);

            // ---- objectives panel -------------------------------------------
            RectTransform objectivesPanel = MakePanel(root, "ObjectivesPanel", new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(580f, 340f),
                PanelDark, false).rectTransform;

            Text objectivesTitle = MakeText(objectivesPanel, "Title", "OBJECTIVES", 20, Warm, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -18f), new Vector2(500f, 28f));
            Localize(objectivesTitle);

            Text objectivesText = MakeText(objectivesPanel, "List", "", 20, Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -58f), new Vector2(530f, 258f));

            // ---- mobile controls --------------------------------------------
            GameObject mobileRoot = MakeRect(root, "MobileControls", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero).gameObject;

            RectTransform joystickBackground = MakeImage(mobileRoot.transform, "JoystickBackground",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(190f, 190f), new Vector2(240f, 240f), KnobSprite,
                new Color(0.08f, 0.08f, 0.09f, 0.55f), true).rectTransform;

            Image joystickHandle = MakeImage(joystickBackground, "Handle", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f),
                KnobSprite, new Color(0.92f, 0.86f, 0.70f, 0.38f), false);

            TouchJoystick joystick = joystickBackground.gameObject.AddComponent<TouchJoystick>();
            joystick.background = joystickBackground;
            joystick.handle = joystickHandle.rectTransform;
            joystick.radius = 90f;

            AddMobileButton(mobileRoot.transform, "InteractButton", "E", TouchButton.Kind.Interact,
                new Vector2(-190f, 300f), new Vector2(150f, 92f));
            AddMobileButton(mobileRoot.transform, "FlashlightButton", "LIGHT", TouchButton.Kind.Flashlight,
                new Vector2(-190f, 195f), new Vector2(150f, 92f));
            AddMobileButton(mobileRoot.transform, "SprintButton", "RUN", TouchButton.Kind.SprintHold,
                new Vector2(-350f, 300f), new Vector2(140f, 80f));
            AddMobileButton(mobileRoot.transform, "CrouchButton", "CROUCH", TouchButton.Kind.CrouchToggle,
                new Vector2(-350f, 195f), new Vector2(140f, 80f));
            AddMobileButton(mobileRoot.transform, "ObjectivesButton", "TASKS", TouchButton.Kind.Objectives,
                new Vector2(-190f, 105f), new Vector2(150f, 56f));
            AddMobileButton(mobileRoot.transform, "PauseButton", "II", TouchButton.Kind.Pause,
                new Vector2(-350f, 105f), new Vector2(140f, 56f));

            mobileRoot.SetActive(false);

            // ---- quiz overlay -----------------------------------------------
            QuizUIController quiz = BuildQuiz(root);

            // ---- pause overlay ----------------------------------------------
            BuildPause(root, pause);
            pause.settings = BuildSettingsPanel(canvas);

            // ---- fade (above everything) ------------------------------------
            Image fade = MakeImage(root, "Fade", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, null, new Color(0f, 0f, 0f, 0f), false);
            fade.gameObject.SetActive(false);

            hud.meterFill = meterFill;
            hud.batteryFill = batteryFill;
            hud.promptText = prompt;
            hud.messageText = message;
            hud.objectivesPanel = objectivesPanel.gameObject;
            hud.objectivesText = objectivesText;
            hud.crosshair = crosshair.gameObject;
            hud.fadeImage = fade;

            TouchInput touch = canvas.gameObject.AddComponent<TouchInput>();
            touch.player = player != null ? player.GetComponent<PlayerController>() : null;
            touch.mobileHudRoot = mobileRoot;

            return new SchoolUI
            {
                canvas = canvas,
                hud = hud,
                quiz = quiz,
                pause = pause,
                touch = touch,
                mobileRoot = mobileRoot
            };
        }

        static QuizUIController BuildQuiz(Transform root)
        {
            RectTransform dim = MakeFullScreen(root, "QuizPanel", new Color(0f, 0f, 0f, 0.82f), true);
            QuizUIController quiz = dim.gameObject.AddComponent<QuizUIController>();

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040f, 800f), PanelDark, false).rectTransform;

            Text header = MakeText(card, "Header", "", 21, Warm, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(44f, -28f), new Vector2(760f, 30f));

            Text timer = MakeText(card, "Timer", "", 24, Warm, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-44f, -28f), new Vector2(160f, 32f));

            Text question = MakeText(card, "Question", "", 31, Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(44f, -80f), new Vector2(952f, 140f));

            Button[] buttons = new Button[4];
            Text[] labels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                Text label;
                buttons[i] = MakeButton(card, "Option" + (i + 1), (i + 1) + ". Option " + (i + 1),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(44f, -236f - i * 82f), new Vector2(952f, 68f), ButtonDark, 23, out label);
                labels[i] = label;
                label.alignment = TextAnchor.MiddleLeft;
            }

            RectTransform feedbackPanel = MakePanel(card, "Feedback", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(44f, -572f), new Vector2(952f, 158f),
                new Color(0.10f, 0.11f, 0.10f, 0.98f), false).rectTransform;

            Text feedback = MakeText(feedbackPanel, "FeedbackText", "", 21, Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(-36f, 126f));

            MakeText(card, "Footer", "1-4 or click to answer   ·   the world does not pause", 16, Dim,
                TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(-80f, 26f));
            Localize(card.Find("Footer").GetComponent<Text>());

            quiz.panel = dim.gameObject;
            quiz.headerText = header;
            quiz.questionText = question;
            quiz.timerText = timer;
            quiz.optionButtons = buttons;
            quiz.optionTexts = labels;
            quiz.feedbackPanel = feedbackPanel.gameObject;
            quiz.feedbackText = feedback;
            feedbackPanel.gameObject.SetActive(false);

            dim.gameObject.SetActive(false);
            return quiz;
        }

        static void BuildPause(Transform root, PauseMenu pause)
        {
            RectTransform dim = MakeFullScreen(root, "PausePanel", new Color(0f, 0f, 0f, 0.74f), true);
            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 520f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "PAUSED", 52, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -48f), new Vector2(580f, 70f), FontStyle.Bold, true);
            Localize(title);

            Text sub = MakeText(card, "Sub", "The school waits.", 20, Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -108f), new Vector2(580f, 32f), FontStyle.Italic);
            Localize(sub);

            Text resumeLabel;
            Button resume = MakeButton(card, "Resume", "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(420f, 64f), ButtonWarm, 24, out resumeLabel);
            Localize(resumeLabel);

            Text settingsLabel;
            Button settings = MakeButton(card, "Settings", "SETTINGS", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(420f, 58f),
                ButtonDark, 21, out settingsLabel);
            Localize(settingsLabel);

            Text quitLabel;
            Button quit = MakeButton(card, "Quit", "QUIT TO DESKTOP", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -124f), new Vector2(420f, 58f),
                ButtonDark, 19, out quitLabel);
            Localize(quitLabel);

            Text hint = MakeText(card, "Hint", "Esc or the pause button closes this card.", 15, Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 22f), new Vector2(580f, 24f));
            Localize(hint);

            pause.panel = dim.gameObject;
            pause.resumeButton = resume;
            pause.settingsButton = settings;
            pause.quitButton = quit;
            dim.gameObject.SetActive(false);
        }

        static void AddMobileButton(Transform parent, string name, string label, TouchButton.Kind kind,
            Vector2 position, Vector2 size)
        {
            Text labelText;
            Button button = MakeButton(parent, name, label, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0.5f), position, size, ButtonDark, 20, out labelText);
            labelText.fontStyle = FontStyle.Bold;
            Localize(labelText);

            TouchButton touch = button.gameObject.AddComponent<TouchButton>();
            touch.kind = kind;
            touch.player = null;
        }

        // ============================================================ settings

        public static SettingsPanelController BuildSettingsPanel(Canvas canvas)
        {
            Transform root = canvas.transform;
            RectTransform dim = MakeFullScreen(root, "SettingsPanel", new Color(0f, 0f, 0f, 0.88f), true);

            SettingsPanelController controller = canvas.gameObject.AddComponent<SettingsPanelController>();
            controller.panel = dim.gameObject;
            controller.rowLabels = new Text[8];
            controller.rowValues = new Text[8];
            controller.prevButtons = new Button[8];
            controller.nextButtons = new Button[8];

            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1120f, 840f), PanelDark, false).rectTransform;

            controller.title = MakeText(card, "Title", "SETTINGS", 44, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -42f), new Vector2(900f, 58f), FontStyle.Bold, true);

            string[] rowKeys =
            {
                "Resolution", "Fullscreen", "Quality", "VSync",
                "Brightness", "Mouse sensitivity", "Language", "Narrator voice"
            };

            for (int i = 0; i < rowKeys.Length; i++)
            {
                float y = -136f - i * 70f;

                controller.rowLabels[i] = MakeText(card, "RowLabel" + i, rowKeys[i], 22, Paper,
                    TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(64f, y), new Vector2(430f, 36f));

                controller.rowValues[i] = MakeText(card, "RowValue" + i, "", 22, Warm,
                    TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(520f, y), new Vector2(310f, 36f));

                Text prevLabel;
                controller.prevButtons[i] = MakeButton(card, "Prev" + i, "<", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(846f, y + 3f), new Vector2(44f, 40f),
                    ButtonDark, 22, out prevLabel);

                Text nextLabel;
                controller.nextButtons[i] = MakeButton(card, "Next" + i, ">", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1006f, y + 3f), new Vector2(44f, 40f),
                    ButtonDark, 22, out nextLabel);
            }

            Text resetLabel;
            controller.resetButton = MakeButton(card, "Reset", "Reset defaults", new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(64f, 48f), new Vector2(330f, 58f),
                ButtonDark, 19, out resetLabel);
            controller.resetLabel = resetLabel;

            Text closeLabel;
            controller.closeButton = MakeButton(card, "Close", "Close", new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-64f, 48f), new Vector2(300f, 58f),
                ButtonWarm, 20, out closeLabel);
            controller.closeLabel = closeLabel;

            dim.gameObject.SetActive(false);
            return controller;
        }

        // ============================================================= main menu

        public static MainMenuController BuildMainMenuUI()
        {
            Canvas canvas = CreateCanvas("UI_MainMenu");
            Transform root = canvas.transform;
            MainMenuController controller = canvas.gameObject.AddComponent<MainMenuController>();

            MakeFullScreen(root, "Background", new Color(0.012f, 0.014f, 0.019f, 1f), false);
            MakeImage(root, "Glow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1500f, 1000f), KnobSprite,
                new Color(1f, 0.72f, 0.36f, 0.055f), false);

            Text eyebrow = MakeText(root, "Eyebrow", "AL-NOOR SECONDARY SCHOOL  ·  THE LAST NIGHT", 18,
                new Color(0.62f, 0.58f, 0.50f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 105f), new Vector2(1200f, 30f));
            Localize(eyebrow);

            Text title = MakeText(root, "Title", "LIGHT SHADOW", 108, new Color(0.97f, 0.91f, 0.79f),
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 300f), new Vector2(1500f, 140f), FontStyle.Bold, true);
            Localize(title);

            Text subtitle = MakeText(root, "Subtitle",
                "Trapped in her dead mother's school on the night before demolition,\na grieving student must recharge a knowledge-powered flashlight by answering real questions.",
                24, new Color(0.72f, 0.70f, 0.66f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 175f), new Vector2(1100f, 90f), FontStyle.Italic);
            Localize(subtitle);

            Text playLabel;
            Button play = MakeButton(root, "PlayButton", "ENTER THE SCHOOL", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(430f, 80f),
                ButtonWarm, 24, out playLabel);
            Localize(playLabel);

            Text howLabel;
            Button how = MakeButton(root, "HowToButton", "HOW TO PLAY", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(430f, 58f),
                ButtonDark, 20, out howLabel);
            Localize(howLabel);

            Text settingsLabel;
            Button settingsButton = MakeButton(root, "SettingsButton", "SETTINGS", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -144f), new Vector2(430f, 54f),
                ButtonDark, 20, out settingsLabel);
            Localize(settingsLabel);

            Text quitLabel;
            Button quit = MakeButton(root, "QuitButton", "QUIT", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -214f), new Vector2(430f, 50f),
                ButtonDark, 19, out quitLabel);
            Localize(quitLabel);

            Text footer = MakeText(root, "Footer", "A game for good  ·  no combat, no gore  ·  one night, one flashlight",
                16, new Color(0.45f, 0.44f, 0.42f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 30f), new Vector2(1200f, 26f));
            Localize(footer);

            controller.playButton = play;
            controller.howToButton = how;
            controller.settingsButton = settingsButton;
            controller.quitButton = quit;
            controller.howToPanel = BuildHowTo(root, controller);
            controller.settings = BuildSettingsPanel(canvas);

            return controller;
        }

        static GameObject BuildHowTo(Transform root, MainMenuController controller)
        {
            RectTransform dim = MakeFullScreen(root, "HowToPanel", new Color(0f, 0f, 0f, 0.86f), true);
            RectTransform card = MakePanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 700f), PanelDark, false).rectTransform;

            Text title = MakeText(card, "Title", "HOW TO PLAY", 42, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -44f), new Vector2(880f, 56f), FontStyle.Bold, true);
            Localize(title);

            string body =
                "You are Nour. The school is dark, the demolition crew comes at dawn, and the Shadow hunts in the dark.\n\n" +
                "<b>Move</b>  W A S D        <b>Look</b>  mouse\n" +
                "<b>Sprint</b>  hold Left Shift    <b>Crouch</b>  hold Left Ctrl\n" +
                "<b>Flashlight</b>  F            <b>Interact</b>  E\n" +
                "<b>Answer</b>  1 2 3 4          <b>Objectives</b>  Tab      <b>Pause</b>  Esc\n\n" +
                "Answer chalkboard questions to charge the <b>Bright Idea Meter</b>. A brighter beam reaches further, " +
                "repels the Shadow above 30, and keeps the dark away. Wrong answers and timeouts dim it.\n\n" +
                "Lit rooms are safe: the Shadow cannot enter them, and every lit room is a checkpoint. The battery is " +
                "fuel and runs out; batteries are scattered through the school.\n\n" +
                "Restore <b>three fuse boxes</b>, collect <b>six Knowledge Fragments</b>, power the gate, and walk out. " +
                "You cannot die - being caught only costs light and sends you back to the last checkpoint.";

            string arabicBody =
                "أنت نور. المدرسة مظلمة، وفريق الهدم يأتي عند الفجر، والظل يطارد في العتمة.\n\n" +
                "الحركة: W A S D        النظر: الفأرة\n" +
                "الجري: استمر بالضغط على Shift الأيسر    الانخفاض: Ctrl الأيسر\n" +
                "المصباح: F            التفاعل: E\n" +
                "الإجابة: 1 2 3 4          المهام: Tab      الإيقاف: Esc\n\n" +
                "أجب عن أسئلة السبورة لشحن مؤشر الفكرة المضيئة. الشعاع الأقوى يصل أبعد، ويطرد الظل فوق 30، ويبقي العتمة بعيدة. الإجابات الخاطئة وانتهاء الوقت تخفت الشعاع.\n\n" +
                "الغرف المضاءة آمنة: لا يستطيع الظل دخولها، وكل غرفة مضاءة نقطة حفظ. البطارية وقود وتنفد؛ البطاريات موزعة في المدرسة.\n\n" +
                "أعد تشغيل ثلاثة صناديق مصاهر، واجمع ست شذرات معرفة، وشغّل البوابة، واخرج. لا يمكنك الموت — الإمساك بك يكلفك ضوءًا ويعيدك إلى آخر نقطة حفظ.";

            Text bodyText = MakeText(card, "Body", body, 21, Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -118f), new Vector2(-96f, 480f));
            Localize(bodyText, arabicBody, true);

            Text closeLabel;
            Button close = MakeButton(card, "CloseButton", "BACK", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(300f, 56f),
                ButtonWarm, 21, out closeLabel);
            Localize(closeLabel);
            controller.closeHowToButton = close;

            dim.gameObject.SetActive(false);
            return dim.gameObject;
        }

        // ================================================================ ending

        public static EndingController BuildEndingUI()
        {
            Canvas canvas = CreateCanvas("UI_Ending");
            Transform root = canvas.transform;
            EndingController ending = canvas.gameObject.AddComponent<EndingController>();

            MakeFullScreen(root, "Background", new Color(0.060f, 0.052f, 0.045f, 1f), false);
            MakeImage(root, "DawnGlow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 330f), new Vector2(1800f, 1000f), KnobSprite,
                new Color(1f, 0.55f, 0.25f, 0.16f), false);

            var dawnRoot = new GameObject("Dawn", typeof(RectTransform), typeof(CanvasGroup));
            dawnRoot.transform.SetParent(root, false);
            RectTransform dawnRect = dawnRoot.GetComponent<RectTransform>();
            Stretch(dawnRect);

            Text dawnTitle = MakeText(dawnRoot.transform, "DawnTitle", "DAWN", 112, Warm, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 280f), new Vector2(1200f, 150f), FontStyle.Bold, true);
            Localize(dawnTitle);

            Text dawnBody = MakeText(dawnRoot.transform, "DawnBody",
                "The gate closes behind her. The sky above the school turns from black to grey to a thin, clean gold.\n\n" +
                "She opens the memory box on the pavement and reads her mother's lesson plans one last time.",
                26, new Color(0.84f, 0.81f, 0.74f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(1160f, 320f));
            Localize(dawnBody);

            // ---- recap -------------------------------------------------------
            RectTransform recapPanel = MakeFullScreen(root, "RecapPanel", new Color(0.030f, 0.031f, 0.036f, 0.99f), true);
            RecapScreen recap = canvas.gameObject.AddComponent<RecapScreen>();

            Text title = MakeText(recapPanel, "RecapTitle", "Study Recap", 56, Warm, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(70f, -44f), new Vector2(1000f, 70f), FontStyle.Bold, true);

            Text stats = MakeText(recapPanel, "RecapStats", "", 22, Paper, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(72f, -120f), new Vector2(1200f, 34f));

            Text bodyText;
            CreateScroll(recapPanel, new Vector2(70f, -170f), new Vector2(1780f, 640f), out bodyText);

            Text menuLabel;
            Button menu = MakeButton(recapPanel, "MenuButton", "RETURN TO MAIN MENU",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-260f, 48f), new Vector2(380f, 62f), ButtonDark, 19, out menuLabel);
            Localize(menuLabel);

            Text quitLabel;
            Button quit = MakeButton(recapPanel, "QuitButton", "QUIT",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(180f, 48f), new Vector2(260f, 62f), ButtonWarm, 20, out quitLabel);
            Localize(quitLabel);

            Text footer = MakeText(recapPanel, "Footer",
                "\"Knowledge is the only light that follows a person into the dark.\"",
                21, new Color(0.66f, 0.62f, 0.55f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 130f), new Vector2(1300f, 30f), FontStyle.Italic);
            Localize(footer);

            recap.panel = recapPanel.gameObject;
            recap.titleText = title;
            recap.statsText = stats;
            recap.recapText = bodyText;
            recap.menuButton = menu;
            recap.quitButton = quit;
            recapPanel.gameObject.SetActive(false);

            ending.dawnTitle = dawnTitle;
            ending.dawnBody = dawnBody;
            ending.dawnGroup = dawnRoot.GetComponent<CanvasGroup>();
            ending.recap = recap;

            return ending;
        }

        // ================================================================ helpers

        static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            go.AddComponent<UIFontBinder>();
            return canvas;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static RectTransform MakeRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        static RectTransform MakeFullScreen(Transform parent, string name, Color color, bool raycast)
        {
            RectTransform rect = MakeRect(parent, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return rect;
        }

        static Image MakeImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, Sprite sprite, Color color, bool raycast)
        {
            RectTransform rect = MakeRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            if (sprite == BackgroundSprite) image.type = Image.Type.Sliced;
            return image;
        }

        static Image MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, Color color, bool raycast)
        {
            return MakeImage(parent, name, anchorMin, anchorMax, pivot, position, size, BackgroundSprite, color, raycast);
        }

        static Text MakeText(Transform parent, string name, string content, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position,
            Vector2 size, FontStyle style = FontStyle.Normal, bool shadow = false)
        {
            RectTransform rect = MakeRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = UIFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.08f;
            text.supportRichText = true;
            text.raycastTarget = false;

            if (shadow)
            {
                Shadow effect = rect.gameObject.AddComponent<Shadow>();
                effect.effectColor = new Color(0f, 0f, 0f, 0.85f);
                effect.effectDistance = new Vector2(2f, -2f);
            }

            return text;
        }

        static void Localize(Text text, string arabic = null, bool rightAlign = false)
        {
            LocalizedText.Attach(text, arabic, rightAlign);
        }

        static void LocalizeWorld(TextMesh text, string arabic = null)
        {
            LocalizedWorldText.Attach(text, arabic);
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, Color background, int fontSize, out Text labelText)
        {
            RectTransform rect = MakeRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = BackgroundSprite;
            image.type = Image.Type.Sliced;
            image.color = background;
            image.raycastTarget = true;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.12f, 1.05f, 1f);
            colors.pressedColor = new Color(0.78f, 0.76f, 0.72f, 1f);
            colors.selectedColor = new Color(1.05f, 1.02f, 0.98f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            labelText = MakeText(rect, "Label", label, fontSize, Paper, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            labelText.raycastTarget = false;

            return button;
        }

        static Image MakeBar(Transform parent, string name, Vector2 topLeft, Vector2 size, Color fillColor,
            string label, out Text labelText)
        {
            labelText = MakeText(parent, name + "Label", label, 16, new Color(0.68f, 0.65f, 0.60f),
                TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                topLeft, new Vector2(size.x, 22f));
            Localize(labelText);

            Image background = MakeImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), topLeft + new Vector2(0f, -22f), size, BackgroundSprite,
                new Color(0.06f, 0.06f, 0.07f, 0.92f), false);

            Image fill = MakeImage(background.transform, "Fill", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, UISprite, fillColor, false);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;

            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(1f, -1f);

            return fill;
        }

        static void CreateScroll(Transform parent, Vector2 position, Vector2 size, out Text bodyText)
        {
            RectTransform root = MakeRect(parent, "RecapScroll", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), position, size);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = BackgroundSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.05f, 0.05f, 0.06f, 0.9f);
            background.raycastTarget = true;

            ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;

            RectTransform viewport = MakeRect(root, "Viewport", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-36f, -28f));
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewportImage.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = MakeRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);

            bodyText = content.gameObject.AddComponent<Text>();
            bodyText.font = UIFont;
            bodyText.fontSize = 20;
            bodyText.color = Paper;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.lineSpacing = 1.18f;
            bodyText.supportRichText = true;
            bodyText.raycastTarget = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
        }
    }
}
