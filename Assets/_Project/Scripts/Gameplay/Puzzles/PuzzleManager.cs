using System;
using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Registry and save bridge for the wing puzzles. Lives in the school scene
    // next to the GameManager. Puzzles register themselves in Start; solved ids
    // ride the save blob through SaveSystem's capture/apply events, the same
    // pattern the story system uses for storyJson (the engine copies the blob
    // verbatim and never interprets it).
    //
    // Restore timing: SaveApplied fires from the sceneLoaded callback, before
    // scene objects run Start, so puzzles are not registered yet when the blob
    // arrives. The manager therefore just keeps the solved id set, and each
    // PuzzleBase picks up its restored state when it registers.
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance { get; private set; }

        readonly Dictionary<string, PuzzleBase> byId = new Dictionary<string, PuzzleBase>();
        readonly HashSet<string> solved = new HashSet<string>();

        [Serializable]
        class PuzzleSave
        {
            public List<string> solvedIds = new List<string>();
        }

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        void OnDisable()
        {
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Register(PuzzleBase puzzle)
        {
            if (puzzle == null || string.IsNullOrEmpty(puzzle.puzzleId)) return;
            byId[puzzle.puzzleId] = puzzle;
            if (solved.Contains(puzzle.puzzleId)) puzzle.RestoreSolved();
        }

        public bool IsSolved(string id)
        {
            return id != null && solved.Contains(id);
        }

        public void MarkSolved(string id)
        {
            if (!string.IsNullOrEmpty(id)) solved.Add(id);
        }

        void OnSaveCaptured(SaveSystem.SaveData data)
        {
            if (data == null) return;
            PuzzleSave save = new PuzzleSave();
            save.solvedIds.AddRange(solved);
            data.puzzleJson = JsonUtility.ToJson(save);
        }

        void OnSaveApplied(SaveSystem.SaveData data)
        {
            solved.Clear();
            if (data == null || string.IsNullOrEmpty(data.puzzleJson)) return;
            try
            {
                PuzzleSave save = JsonUtility.FromJson<PuzzleSave>(data.puzzleJson);
                if (save != null && save.solvedIds != null)
                {
                    for (int i = 0; i < save.solvedIds.Count; i++) solved.Add(save.solvedIds[i]);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Puzzle save blob unreadable, starting puzzles fresh: " + exception.Message);
            }
        }
    }
}
