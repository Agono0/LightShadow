using System.Collections.Generic;
using LightShadow.Enemy;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Core
{
    // The conductor of the night: keeps score (fuse boxes, fragments, every
    // answered question), sets quiz difficulty, and handles the big beats -
    // restore, caught, gate powered, escape at dawn (Doc 4.3 / 4.4).
    public class GameManager : MonoBehaviour
    {
        // One answered (or skipped) question, kept for the study recap.
        public struct AnswerRecord
        {
            public QuestionData question;
            public int chosenIndex; // -1 means the timer ran out
            public bool correct;
        }

        public const int FuseTarget = 3;
        public const int FragmentTarget = 6;

        public static GameManager Instance { get; private set; }

        [Header("Wiring")]
        public GameConfig config;
        public Transform player;
        public EnemyBehaviour enemy;
        public HUDController hud;
        public QuizUIController quizUI;
        public RecapScreen recapScreen;
        public QuestionDatabase database;

        [Header("Audio")]
        public AudioClip caughtClip;

        public int FusesRestored { get; private set; }
        public int FragmentsCollected { get; private set; }
        public bool GatePowered { get; private set; }

        readonly List<AnswerRecord> answers = new List<AnswerRecord>();
        public List<AnswerRecord> Answers => answers;

        bool respawning;
        bool escaped;

        // Doc 4.4: Act I recall -> Act II apply (one fuse) -> Act III mixed (all).
        public QuestionDifficulty CurrentDifficulty
        {
            get
            {
                if (FusesRestored >= FuseTarget) return QuestionDifficulty.ActIII;
                if (FusesRestored >= 1) return QuestionDifficulty.ActII;
                return QuestionDifficulty.ActI;
            }
        }

        public bool CanPowerGate => FusesRestored >= FuseTarget;

        void Awake()
        {
            Instance = this;
            if (config == null)
            {
                // So the game still runs if the asset was never assigned.
                config = ScriptableObject.CreateInstance<GameConfig>();
                Debug.LogWarning("LightShadow: no GameConfig assigned, using code defaults.");
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            // A previous run may have been paused or slowed when the scene ended.
            Time.timeScale = 1f;
            if (database != null) database.ResetBag();
            if (hud != null) hud.RefreshObjectives();

            // The narrator (Windows SAPI voice) opens the night once. Audio is
            // quiet enough not to hide the Shadow's cues; the option is offable
            // in Settings.
            StoryTeller.Speak(Localization.Tr(
                "The school remembers her mother. Nour must carry what is precious out before dawn."), true);
        }

        // ---- Small services other scripts call --------------------------------

        public void ShowMessage(string text, float seconds = 4f)
        {
            if (hud != null) hud.ShowMessage(text, seconds);
        }

        public BrightIdeaMeter GetMeter()
        {
            return player != null ? player.GetComponentInChildren<BrightIdeaMeter>() : null;
        }

        // Used by fragments and memory flashes (meter + recap in one call).
        public void ApplyQuizResult(QuestionData question, int chosenIndex, bool correct)
        {
            BrightIdeaMeter meter = GetMeter();
            if (meter != null)
            {
                if (correct) meter.AddCorrect();
                else meter.AddWrong();
            }
            RecordAnswer(question, chosenIndex, correct);
        }

        // Doc 4.1: the study recap remembers every question of the run.
        public void RecordAnswer(QuestionData question, int chosenIndex, bool correct)
        {
            answers.Add(new AnswerRecord
            {
                question = question,
                chosenIndex = chosenIndex,
                correct = correct
            });
        }

        // ---- Story beats -------------------------------------------------------

        public void OnFuseRestored(string memoryFlash)
        {
            FusesRestored++;
            if (!string.IsNullOrEmpty(memoryFlash)) ShowMessage(memoryFlash, 4f);
            if (hud != null) hud.RefreshObjectives();
        }

        public void OnFragmentCollected()
        {
            FragmentsCollected++;
            if (hud != null) hud.RefreshObjectives();
        }

        public void OnGatePowered()
        {
            GatePowered = true;
            // Doc 4.4: the Shadow follows Nour through the whole main hall at dawn.
            if (enemy != null) enemy.SetAggressive(true);
            ShowMessage("The gate is powered. Run.", 5f);
            if (hud != null) hud.RefreshObjectives();
        }

        // Escaping ends the run: fade to black, then 03_Ending shows the study recap.
        public void Escape()
        {
            if (escaped) return;
            escaped = true;

            if (enemy != null) enemy.enabled = false;

            // The answer list must survive the scene change to 03_Ending.
            RunResults.CopyFrom(answers);

            if (hud != null) hud.FadeToBlack(2f, GoToEnding);
            else GoToEnding();
        }

        void GoToEnding()
        {
            // If the ending scene is missing (for example a developer pressed Play
            // directly in 02_School before running the scene builder), still show
            // the recap instead of breaking the run.
            if (Application.CanStreamedLevelBeLoaded(SceneNames.Ending))
            {
                SceneManager.LoadScene(SceneNames.Ending);
                return;
            }

            ShowRecap();
        }

        void ShowRecap()
        {
            if (recapScreen != null) recapScreen.Show(answers);
        }

        // Doc 4.3: caught -> quick fade, lose 15 meter, wake at the last lit checkpoint.
        public void CaughtPlayer()
        {
            if (respawning || escaped) return;
            respawning = true;

            if (caughtClip == null) caughtClip = ProceduralSfx.Wrong;
            if (caughtClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(caughtClip);
            }

            BrightIdeaMeter meter = GetMeter();
            if (meter != null && config != null) meter.RemoveMeter(config.caughtMeterPenalty);

            if (hud != null) hud.FadeToBlack(1f, WakeAtCheckpoint);
            else WakeAtCheckpoint();
        }

        void WakeAtCheckpoint()
        {
            Transform spawn = CheckpointManager.Instance != null ? CheckpointManager.Instance.SpawnPoint : null;
            if (spawn != null && player != null)
            {
                // CharacterController fights direct moves; switch it off for the hop.
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                player.SetPositionAndRotation(spawn.position, spawn.rotation);
                if (controller != null) controller.enabled = true;
            }

            if (enemy != null) enemy.ResetToPatrol();

            if (hud != null)
            {
                hud.FadeIn(1f);
                string line = CheckpointManager.Instance != null ? CheckpointManager.Instance.ChalkLine : "";
                if (!string.IsNullOrEmpty(line)) hud.ShowMessage(line, 4f);
            }

            respawning = false;
        }
    }
}
