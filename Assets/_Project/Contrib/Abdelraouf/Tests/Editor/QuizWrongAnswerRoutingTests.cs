using NUnit.Framework;
using UnityEngine;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // Regression coverage for the S1 board answer-event pipeline (audit v3,
    // fixed in f0556cc, reviewed at main bc7e6e14155e2675b650ae5a6c6b2df73c6002e8):
    // a board answer must flow through GameManager.ApplyQuizResult exactly
    // once - the meter, the study recap, and the QuestionAnswered event the
    // story, the ending accuracy and the wrong-answer buzzer listen to.
    //
    // Distinct from Tests/Editor/QuizBoardRoutingTests.cs (owner's), which
    // covers the CORRECT arm with its own fixture. This file covers exactly
    // one case the existing suites do not: the WRONG arm - AddWrong penalty,
    // one recap entry marked wrong, one QuestionAnswered event carrying
    // false. The cancel-neutral contract is exercised through the real seam
    // QuizUIController.IsCancel as a scenario guard only; the full cancel
    // mapping (timeout -1, all four options) is owned by
    // Tests/Editor/QuizCancelTests.cs and is not repeated here.
    //
    // NOT covered here (editor limits, stated honestly): QuizBoard.Interact
    // and its ShowQuestion callback are wired to a live QuizUIController and
    // QuestionDatabase; driving them needs a scene, so the board wiring
    // itself is not faked. The correct arm and the IsCancel mapping stay
    // with the existing suites.
    //
    // EXECUTION STATUS: written by inspection against the sources at main
    // bc7e6e14155e2675b650ae5a6c6b2df73c6002e8; NOT compiled or executed by
    // the author (no Unity editor available where this was written).
    // Static expected-pass is not a pass. No Assert.Inconclusive anywhere.
    //
    // FIXTURE SAFETY (verified against the source at the base above):
    //  * The GameManager host GameObject is created INACTIVE before
    //    AddComponent, so GameManager.Awake/Start never run: the static
    //    Instance is never assigned, no GameConfig is created, and the
    //    Start-time side effects (Time.timeScale, StoryTeller/SAPI,
    //    AudioManager, Debug logging) never execute.
    //  * The meter sits on a separate ACTIVE player root because
    //    GetComponentInChildren skips inactive objects (GameManager.GetMeter
    //    resolves the meter through the public player field only).
    //    BrightIdeaMeter.Awake on that root only assigns the in-memory
    //    ProceduralSfx clips and keeps the default meter value: at that
    //    moment GameManager.Instance is still null, so config stays null
    //    and every code default applies (start 60, wrong penalty 10,
    //    max 100). Start/Update never run in EditMode, ApplyToLight
    //    returns on a null spotLight, AudioManager.Instance is null, and
    //    the haptics call (correct arm only, never hit here) is compiled
    //    out in the editor.
    //  * Isolation: the only shared state touched is the static
    //    GameEvents.QuestionAnswered subscription, removed in finally; the
    //    recap count is snapshotted before the call and asserted relative,
    //    never cleared. No prefs, no statics overwritten, no scene access.
    public class QuizWrongAnswerRoutingTests
    {
        [Test]
        public void WrongAnswer_AddsPenalty_RecordsOnce_RaisesEventFalse_CancelStaysNeutral()
        {
            // The board callback routes an answer to ApplyQuizResult only
            // when the real cancel seam rejects it; guard this scenario on
            // that seam (CancelIndex filtered, a real option index routed).
            Assert.IsTrue(QuizUIController.IsCancel(QuizUIController.CancelIndex));
            Assert.IsFalse(QuizUIController.IsCancel(2));

            GameObject gmHost = new GameObject("QWA_GameManager");
            gmHost.SetActive(false);
            GameObject player = new GameObject("QWA_Player");
            QuestionData question = ScriptableObject.CreateInstance<QuestionData>();
            int raised = 0;
            bool lastCorrect = true;
            System.Action<bool> handler = c => { raised++; lastCorrect = c; };
            GameEvents.QuestionAnswered += handler;
            try
            {
                BrightIdeaMeter meter = player.AddComponent<BrightIdeaMeter>();
                GameManager gm = gmHost.AddComponent<GameManager>();
                gm.player = player.transform;

                float meterBefore = meter.Meter;
                int answersBefore = gm.Answers.Count;

                gm.ApplyQuizResult(question, 2, false);

                Assert.AreEqual(meterBefore - 10f, meter.Meter,
                    "wrong answer applies the default 10-point penalty through AddWrong");
                Assert.AreEqual(answersBefore + 1, gm.Answers.Count,
                    "the recap records the answer exactly once");
                Assert.AreEqual(2, gm.Answers[gm.Answers.Count - 1].chosenIndex);
                Assert.IsFalse(gm.Answers[gm.Answers.Count - 1].correct,
                    "the recap entry is marked wrong");
                Assert.AreEqual(1, raised, "QuestionAnswered fires exactly once");
                Assert.IsFalse(lastCorrect, "the event payload marks the answer wrong");
            }
            finally
            {
                GameEvents.QuestionAnswered -= handler;
                Object.DestroyImmediate(question);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(gmHost);
            }
        }
    }
}
