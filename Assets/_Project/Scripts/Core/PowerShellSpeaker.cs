#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace LightShadow.Core
{
    /// <summary>
    /// One long-lived PowerShell that owns System.Speech and speaks the lines
    /// it is sent (audit 2.7: one powershell.exe per utterance). A line is
    /// "en|base64" or "ar|base64"; the worker answers "done" after speaking.
    /// An interrupt kills the worker and the next line starts a fresh one, so
    /// the process cost is only paid after an interrupt, not per line. If the
    /// worker cannot start or keeps failing, Speak returns false and the caller
    /// uses the old one-process-per-line path. Windows only; not run on a
    /// Windows machine by the author (CI compiles it).
    /// </summary>
    static class PowerShellSpeaker
    {
        const int StartupMillis = 20000;
        const int SpeakMillis = 60000;
        const int MaxFailures = 3;

        static readonly object gate = new object();
        static Process proc;
        static BlockingCollection<string> lines;
        static bool broken;
        static int failures;
        static int interruptEpoch;

        const string WorkerScript = @"
$ErrorActionPreference = 'Continue';
Add-Type -AssemblyName System.Speech;
$s = New-Object System.Speech.Synthesis.SpeechSynthesizer; $s.Volume = 85;
$all = $s.GetInstalledVoices();
$en = $all | Where-Object { $_.VoiceInfo.Culture.Name -like 'en-*' };
$envoice = $en | Where-Object { $_.VoiceInfo.Name -match 'Natural|Neural' } | Select-Object -First 1;
if (-not $envoice) { $envoice = $en | Where-Object { $_.VoiceInfo.Name -match 'Aria|Guy|Jenny|Jessa|Zira|Hazel|Susan' } | Select-Object -First 1 };
if (-not $envoice) { $envoice = $en | Where-Object { $_.VoiceInfo.Gender -eq 'Female' } | Select-Object -First 1 };
if (-not $envoice) { $envoice = $en | Select-Object -First 1 };
$arvoice = $all | Where-Object { $_.VoiceInfo.Culture.Name -like '*ar*' } | Select-Object -First 1;
[Console]::WriteLine('ready');
while ($true) {
  $l = [Console]::In.ReadLine();
  if ($l -eq $null) { break };
  $p = $l.Split('|');
  try {
    $plain = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p[1]));
    $skip = $false;
    if ($p[0] -eq 'ar') {
      if (-not $arvoice) { $skip = $true } else { $s.SelectVoice($arvoice.VoiceInfo.Name); $s.Rate = 0; $lang = 'ar-sa' }
    } else {
      if ($envoice) { $s.SelectVoice($envoice.VoiceInfo.Name) }; $s.Rate = -1; $lang = 'en-us'
    }
    if (-not $skip) {
      $ssml = ""<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='$lang'>"" + ($plain -replace '([.!?…])(\s|$)', '$1<break time=""280ms""/>$2' -replace ',', ',<break time=""140ms""/>') + '</speak>';
      try { $s.SpeakSsml($ssml) } catch { $s.Rate = 0; $s.Speak($plain) }
    }
  } catch { }
  [Console]::WriteLine('done');
}
";

        // True when the worker handled the line (spoke it, or was interrupted).
        // False means the worker is unavailable: use the per-line fallback.
        public static bool Speak(string text, bool isArabic)
        {
            Process p;
            BlockingCollection<string> q;
            int epoch;
            lock (gate)
            {
                if (broken) return false;
                if (proc == null || proc.HasExited)
                {
                    if (!StartWorker()) { broken = true; return false; }
                }
                p = proc;
                q = lines;
                epoch = interruptEpoch;
            }

            string payload = (isArabic ? "ar|" : "en|") + Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            try
            {
                p.StandardInput.WriteLine(payload);
                p.StandardInput.Flush();
            }
            catch (Exception)
            {
                lock (gate)
                {
                    if (epoch != interruptEpoch) return true; // interrupted while sending
                    failures++;
                    if (failures >= MaxFailures) broken = true;
                    KillLocked();
                }
                return false;
            }

            string line;
            while (q.TryTake(out line, SpeakMillis))
            {
                if (line == "done" || line == "eof")
                {
                    if (line == "done") failures = 0;
                    return true;
                }
            }

            // No answer in a minute: drop this worker, the next line restarts it.
            lock (gate) { KillLocked(); }
            return true;
        }

        public static void Interrupt()
        {
            lock (gate)
            {
                interruptEpoch++;
                KillLocked();
            }
        }

        static bool StartWorker()
        {
            try
            {
                string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(WorkerScript));
                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -NonInteractive -EncodedCommand " + encoded,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                Process p = Process.Start(start);
                if (p == null) return false;
                BlockingCollection<string> q = new BlockingCollection<string>();
                p.OutputDataReceived += (sender, args) => { q.Add(args.Data == null ? "eof" : args.Data.Trim()); };
                p.BeginOutputReadLine();

                string line;
                while (q.TryTake(out line, StartupMillis))
                {
                    if (line == "ready")
                    {
                        proc = p;
                        lines = q;
                        return true;
                    }
                    if (line == "eof") break;
                }
                try { if (!p.HasExited) p.Kill(); } catch (Exception) { }
                return false;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.Log("LightShadow: speech worker did not start (" + exception.Message + ").");
                return false;
            }
        }

        static void KillLocked()
        {
            try
            {
                if (proc != null && !proc.HasExited) proc.Kill();
            }
            catch (Exception)
            {
                // already gone
            }
            proc = null;
            lines = null;
        }
    }
}
#endif
