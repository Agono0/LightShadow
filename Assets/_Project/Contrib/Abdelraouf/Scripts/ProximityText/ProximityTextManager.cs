using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.ProximityText
{
    // Single Update loop for every ProximityText in the scene: one Camera.main
    // fetch per frame instead of one per text, and dead entries are pruned.
    // Creates itself when the first text registers; destroys nothing else.
    public class ProximityTextManager : MonoBehaviour
    {
        static ProximityTextManager instance;

        readonly System.Collections.Generic.List<ProximityText> texts =
            new System.Collections.Generic.List<ProximityText>();

        Camera viewer;

        public static void Register(ProximityText text)
        {
            if (text == null) return;
            Ensure();
            if (!instance.texts.Contains(text)) instance.texts.Add(text);
        }

        public static void Unregister(ProximityText text)
        {
            if (instance != null) instance.texts.Remove(text);
        }

        static void Ensure()
        {
            if (instance == null)
            {
                instance = new GameObject("ProximityTextManager").AddComponent<ProximityTextManager>();
            }
        }

        void Update()
        {
            if (viewer == null) viewer = Camera.main;
            float dt = Time.unscaledDeltaTime;

            for (int i = texts.Count - 1; i >= 0; i--)
            {
                ProximityText text = texts[i];
                if (text == null)
                {
                    texts.RemoveAt(i);
                    continue;
                }
                text.Tick(viewer, dt);
            }
        }
    }
}
