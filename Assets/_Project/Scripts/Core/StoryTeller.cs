using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Voice tokens kept from the SAPI scan; the worker switches between
        // them per utterance (Arabic lines need an Arabic voice).
        static object arabicVoiceToken;
        static object englishVoiceToken;

        // Ranking of installed English voices by how natural they sound.
        static int EnglishVoiceRank(string description)
        {
            if (string.IsNullOrEmpty(description)) return 0;
            if (description.IndexOf("Natural", StringComparison.OrdinalIgnoreCase) >= 0) return 5;
            if (description.IndexOf("Neural", StringComparison.OrdinalIgnoreCase) >= 0) return 4;
            string[] good =
            {
                "Aria", "Guy", "Jenny", "Jessa", "Zira", "Hazel", "Susan", "Yoning"
            };
            for (int i = 0; i < good.Length; i++)
            {
                if (description.IndexOf(good[i], StringComparison.OrdinalIgnoreCase) >= 0) return 3;
            }
            return 1;
        }

        // SAPI XML adds small silences after punctuation so the delivery
        // breathes instead of machine-gunning through the sentence.
        static string BuildSapiXml(string text)
        {
            string escaped = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            escaped = System.Text.RegularExpressions.Regex.Replace(escaped, @"([.!?…])(\s|$)",
                "$1<silence msec=\"280\"/>$2");
            escaped = escaped.Replace("،", "<silence msec=\"160\"/>،");
            escaped = escaped.Replace(";", "<silence msec=\"200\"/>;");
            escaped = escaped.Replace(":", "<silence msec=\"200\"/>:");
            escaped = escaped.Replace(",", "<silence msec=\"140\"/>,");
            return escaped;
        }
#endif

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

            try
            {
                voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (voiceType != null) voice = Activator.CreateInstance(voiceType);

                if (voice != null)
                {
                    object voices = voiceType.InvokeMember("GetVoices", BindingFlags.InvokeMethod, null, voice, null);
                    if (voices != null)
                    {
                        int count = (int)voices.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, voices, null);
                        object arabicToken = null;
                        object englishToken = null;
                        int englishRank = 0;
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
                                if (arabicToken == null) arabicToken = token;
                            }
                            else
                            {
                                // Prefer the most natural-sounding English voice
                                // installed on the machine (Windows 11 ships the
                                // "Natural" engines on many systems; older SAPI
                                // voices are the fallback).
                                int rank = EnglishVoiceRank(description);
                                if (rank > englishRank)
                                {
                                    englishToken = token;
                                    englishRank = rank;
                                }
                            }
                        }

                        // Keep both tokens; the loop below switches per utterance.
                        arabicVoiceToken = arabicToken;
                        englishVoiceToken = englishToken ?? arabicToken;
                    }

                    voiceType.InvokeMember("Volume", BindingFlags.SetProperty, null, voice, new object[] { 85 });
                    available = true;
                }
            }
            catch (Exception exception)
            {
                // COM activation is blocked inside the built player; the loop
                // below falls back to PowerShell's System.Speech instead.
                voice = null;
                voiceType = null;
                arabicVoiceToken = null;
                englishVoiceToken = null;
                Debug.Log("LightShadow: direct narrator voice unavailable (" + exception.Message
                    + "); trying the PowerShell speech engine.");
            }

            if (voice == null && !PowerShellSpeechAvailable())
            {
                available = false;
                return;
            }

            available = true;
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
                bool isArabic = ArabicSupport.ContainsArabic(text);
                object token = isArabic ? arabicVoiceToken : englishVoiceToken;
                if (token == null)
                {
                    if (isArabic)
                    {
                        // Do not read Arabic with an English phoneme set.
                        continue;
                    }
                    token = arabicVoiceToken ?? englishVoiceToken;
                    if (token == null) continue;
                }

                try
                {
                    if (voice != null)
                    {
                        voiceType.InvokeMember("Voice", BindingFlags.SetProperty, null, voice, new[] { token });
                        // Slightly slower delivery reads far less robotic; Arabic
                        // voices stay at the default pace or they drag.
                        voiceType.InvokeMember("Rate", BindingFlags.SetProperty, null, voice,
                            new object[] { isArabic ? 0 : -1 });
                        // SVSFIsXML (8) makes SAPI parse the pause tags.
                        voiceType.InvokeMember("Speak", BindingFlags.InvokeMethod, null, voice,
                            new object[] { BuildSapiXml(text), 8 });
                    }
                    else
                    {
                        SpeakViaPowerShell(text);
                    }
                }
                catch (Exception)
                {
                    // One bad utterance must not kill the narrator thread.
                }
            }
        }

        // ------------------------------------------------------------ powershell

        // The built player cannot activate COM objects, so narration goes through
        // a tiny PowerShell script that owns System.Speech. One process per
        // utterance keeps error isolation: a stuck speech line cannot hang the
        // narrator thread either, because every call has a timeout.
        static bool PowerShellSpeechAvailable()
        {
            try
            {
                using (var process = StartPowerShell("Add-Type -AssemblyName System.Speech"))
                {
                    if (process == null) return false;
                    if (!process.WaitForExit(15000)) { process.Kill(); return false; }
                    return process.ExitCode == 0;
                }
            }
            catch (Exception exception)
            {
                Debug.Log("LightShadow: narrator speech engine not reachable (" + exception.Message + ").");
                return false;
            }
        }

        static void SpeakViaPowerShell(string text)
        {
            bool isArabic = ArabicSupport.ContainsArabic(text);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            string lang = isArabic ? "ar-sa" : "en-us";

            string script = "Add-Type -AssemblyName System.Speech;"
                + "$s = New-Object System.Speech.Synthesis.SpeechSynthesizer;"
                + "$s.Volume = 85;";

            // Pick the most natural installed voice: prefer a Windows "Natural"
            // engine, then any matching culture; fall back to the system default.
            if (isArabic)
            {
                script += "$v = $s.GetInstalledVoices() | Where-Object "
                    + "{ $_.VoiceInfo.Culture.Name -like '*ar*' } | Select-Object -First 1;"
                    + "if (-not $v) { exit 0 }; $s.SelectVoice($v.VoiceInfo.Name);";
            }
            else
            {
                // Mirror EnglishVoiceRank: Natural/Neural first, then the known
                // clean desktop voices, then any female English voice. The
                // plain "first installed" pick is often the oldest male SAPI
                // voice, which reads much more robotic.
                script += "$voices = $s.GetInstalledVoices() | Where-Object "
                    + "{ $_.VoiceInfo.Culture.Name -like 'en-*' };"
                    + "$v = $voices | Where-Object { $_.VoiceInfo.Name -match 'Natural|Neural' } "
                    + "| Select-Object -First 1;"
                    + "if (-not $v) { $v = $voices | Where-Object "
                    + "{ $_.VoiceInfo.Name -match 'Aria|Guy|Jenny|Jessa|Zira|Hazel|Susan' } "
                    + "| Select-Object -First 1 };"
                    + "if (-not $v) { $v = $voices | Where-Object "
                    + "{ $_.VoiceInfo.Gender -eq 'Female' } | Select-Object -First 1 };"
                    + "if (-not $v) { $v = $voices | Select-Object -First 1 };"
                    + "if ($v) { $s.SelectVoice($v.VoiceInfo.Name) };"
                    + "$s.Rate = -1;";
            }

            // Spoken as SSML with breathing pauses at punctuation; if the engine
            // rejects the document it falls back to plain speech so nothing
            // is ever lost.
            script += "$plain = [Text.Encoding]::UTF8.GetString("
                + "[Convert]::FromBase64String('" + encoded + "'));"
                + "$ssml = \"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='" + lang + "'>\""
                + " + ($plain -replace '([.!?…])(\\s|$)', '$1<break time=\"280ms\"/>$2'"
                + " -replace ',', ',<break time=\"140ms\"/>') + '</speak>';"
                + "try { $s.SpeakSsml($ssml) } catch { $s.Rate = 0; $s.Speak($plain) }";

            using (var process = StartPowerShell(script))
            {
                if (process == null) return;
                if (!process.WaitForExit(60000)) process.Kill();
            }
        }

        // -EncodedCommand takes Base64 UTF-16LE, so no quoting or escaping layer
        // can mangle the SSML, apostrophes or Arabic text on the way to the shell.
        static System.Diagnostics.Process StartPowerShell(string command)
        {
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
            var start = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -EncodedCommand " + encoded,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            return System.Diagnostics.Process.Start(start);
        }
#endif
    }
}
