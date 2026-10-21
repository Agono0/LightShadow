using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace LightShadow.Core
{
    // Story narration without shipping voice files. On Windows the operating
    // system's SAPI voice is used on a dedicated STA thread (so Unity's main
    // thread never blocks); elsewhere the narrator simply stays silent and the
    // subtitles/cues carry the story. The option can be turned off in Settings.
    public static class StoryTeller
    {
        static bool initialized;
        static bool available;
        static BlockingCollection<string> queue;
        static Thread worker;

        public static bool Available
        {
            get
            {
                EnsureInitialized();
                return available;
            }
        }

        public static void Speak(string text, bool interrupt = false)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (!GameSettings.NarratorEnabled) return;

            EnsureInitialized();
            if (queue == null) return;

            if (interrupt)
            {
                while (queue.TryTake(out _)) { }
            }

            queue.Add(text);
        }

        public static void Stop()
        {
            if (queue == null) return;
            while (queue.TryTake(out _)) { }
        }

        static void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                queue = new BlockingCollection<string>();
                worker = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = "LightShadow StoryTeller"
                };
                worker.SetApartmentState(ApartmentState.STA);
                worker.Start();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("LightShadow: narrator voice unavailable (" + exception.Message + ").");
                available = false;
                queue = null;
            }
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        static void WorkerLoop()
        {
            object voice = null;
            Type voiceType = null;
            bool arabicVoice = false;

            try
            {
                voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (voiceType == null) return;
                voice = Activator.CreateInstance(voiceType);
                if (voice == null) return;

                object voices = voiceType.InvokeMember("GetVoices", BindingFlags.InvokeMethod, null, voice, null);
                if (voices != null)
                {
                    int count = (int)voices.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, voices, null);
                    for (int i = 0; i < count; i++)
                    {
                        object token = voices.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, voices,
                            new object[] { i });
                        string description = (string)token.GetType().InvokeMember("GetDescription",
                            BindingFlags.InvokeMethod, null, token, new object[] { 0 });
                        if (description == null) continue;
                        if (description.IndexOf("Arabic", StringComparison.OrdinalIgnoreCase) >= 0
                            || description.IndexOf("Hoda", StringComparison.OrdinalIgnoreCase) >= 0
                            || description.IndexOf("Naayf", StringComparison.OrdinalIgnoreCase) >= 0
                            || description.IndexOf("Salma", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            voiceType.InvokeMember("Voice", BindingFlags.SetProperty, null, voice, new[] { token });
                            arabicVoice = true;
                            break;
                        }
                    }
                }

                voiceType.InvokeMember("Volume", BindingFlags.SetProperty, null, voice, new object[] { 85 });
                voiceType.InvokeMember("Rate", BindingFlags.SetProperty, null, voice, new object[] { 0 });
                available = true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("LightShadow: narrator voice failed to start (" + exception.Message + ").");
                available = false;
                return;
            }

            while (true)
            {
                string text;
                try
                {
                    text = queue.Take();
                }
                catch (Exception)
                {
                    return;
                }

                if (string.IsNullOrEmpty(text)) continue;
                if (ArabicSupport.ContainsArabic(text) && !arabicVoice)
                {
                    // Do not read Arabic with an English phoneme set.
                    continue;
                }

                try
                {
                    voiceType.InvokeMember("Speak", BindingFlags.InvokeMethod, null, voice, new object[] { text, 0 });
                }
                catch (Exception)
                {
                    // One bad utterance must not kill the narrator thread.
                }
            }
        }
#endif
    }
}
