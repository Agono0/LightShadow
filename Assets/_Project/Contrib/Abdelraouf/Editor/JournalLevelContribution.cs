using LightShadow.Contrib.Abdelraouf.Journal;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    // Contract H2 implementation for the lore journal (package C1).
    // SceneBuilder.BuildAll finds this class by reflection and calls Build on
    // the finished school scene. It adds the journal screen and a few
    // placeholder note pedestals just past the player start so the feature
    // can be tried. Real placements belong to level modules (for example the
    // East Wing, package C4) and the owner's side, not to this file.
    public class JournalLevelContribution : ILevelContribution
    {
        const string NotesPath = "Assets/_Project/Contrib/Abdelraouf/Data/Journal/journal_notes.json";
        const string RootName = "Contrib_Journal";

        public string Id => "abdelraouf.journal";

        // After the contract's own ordering default (0); nothing depends on it.
        public int Order => 100;

        // Placeholder pedestals in front of the start point: (right, forward) in meters.
        static readonly Vector2[] PlaceholderSpots =
        {
            new Vector2(-1.2f, 3.5f),
            new Vector2(0f, 3.5f),
            new Vector2(1.2f, 3.5f),
        };

        public void Build(LevelContext ctx)
        {
            // Idempotent: drop a previous copy if the scene was not rebuilt from scratch.
            Transform old = ctx.DynamicRoot.Find(RootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = new GameObject(RootName) { layer = 0 };
            root.transform.SetParent(ctx.DynamicRoot, false);

            var book = root.AddComponent<JournalBook>();
            book.notesJson = AssetDatabase.LoadAssetAtPath<TextAsset>(NotesPath);
            if (book.notesJson == null)
            {
                Debug.LogError("[LightShadow] Journal notes file missing at " + NotesPath + ", journal will be empty.");
            }

            Vector3 origin = ctx.SpawnPoint != null ? ctx.SpawnPoint.position : Vector3.zero;
            Vector3 right = ctx.SpawnPoint != null ? ctx.SpawnPoint.right : Vector3.right;
            Vector3 forward = ctx.SpawnPoint != null ? ctx.SpawnPoint.forward : Vector3.forward;

            for (int i = 0; i < PlaceholderSpots.Length; i++)
            {
                Vector3 position = origin + right * PlaceholderSpots[i].x + forward * PlaceholderSpots[i].y;
                string id = "note_" + (i + 1).ToString("00");
                BuildPedestalWithNote(root.transform, position, id);
            }
        }

        static void BuildPedestalWithNote(Transform parent, Vector3 floorPosition, string noteId)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "JournalPedestal_" + noteId;
            pedestal.layer = 0;
            pedestal.transform.SetParent(parent, false);
            pedestal.transform.position = floorPosition + new Vector3(0f, 0.45f, 0f);
            pedestal.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);

            GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paper.name = "JournalNote_" + noteId;
            paper.layer = 0;
            paper.transform.SetParent(parent, false);
            paper.transform.position = floorPosition + new Vector3(0f, 0.93f, 0f);
            paper.transform.localScale = new Vector3(0.3f, 0.03f, 0.22f);
            paper.AddComponent<JournalNote>().noteId = noteId;
        }
    }
}
