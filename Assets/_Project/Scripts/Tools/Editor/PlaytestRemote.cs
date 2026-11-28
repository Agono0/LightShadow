using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LightShadow.Tools
{
    // Editor-only playtest remote (Unity pass, 2026-11-28). Lets an agent or a
    // tester drive a real Play session in the editor GUI from outside: write
    // one command per line into Temp/playtest_cmd.txt, results are appended
    // to Logs/playtest.log and screenshots land in Logs/Playtest/. It never
    // ships (Editor folder) and only reacts while the editor is in Play mode,
    // so it cannot change a scene on disk.
    //
    //   Unity.exe -projectPath . -executeMethod LightShadow.Tools.PlaytestRemote.PlayFromBootstrap
    //
    // Commands (whitespace separated, floats use '.'):
    //   shot <name>                     Game view screenshot (UI included)
    //   tp <x> <y> <z> [yaw] [pitch]    move the player (CharacterController safe)
    //   scene <name>                    SceneManager.LoadScene
    //   interact <objectName>           calls IInteractable.Interact on it
    //   static <Type> <Member> [args]   call a static method / read a static member
    //   invoke <object> <Component> <Method> [args]
    //   get <object> <Component> <Member>
    //   set <object> <Component> <Member> <value>
    //   diag                            cursor, time, player, enemy, audio, lights
    //   lights                          every active light with intensity/range
    //   find <substring>                active objects whose name contains it
    //   models                          imported hero OBJ children, pivots and marker bounds
    //   cfg <field> [value]             read/write a GameConfig field (asset edit: persists!)
    //   ui                              active UI graphics with screen rects and colours
    //   timescale <v> / stop
    // Outside Play mode: "play", or "editstatic <Type> <Method>" (e.g. SceneBuilder BuildAll).
    [InitializeOnLoad]
    public static class PlaytestRemote
    {
        const string CommandFile = "Temp/playtest_cmd.txt";
        const string LogFile = "Logs/playtest.log";
        const string ShotFolder = "Logs/Playtest";

        static double nextPoll;

        static PlaytestRemote()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        // Entry point for -executeMethod: opens the bootstrap scene and plays.
        public static void PlayFromBootstrap()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText(LogFile, "playtest remote started " + DateTime.Now.ToString("s") + "\n");
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Bootstrap.unity", OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        // -executeMethod LightShadow.Tools.PlaytestRemote.ModelReportBatch (works with -nographics).
        public static void ModelReportBatch()
        {
            Debug.Log("[LightShadow] MODEL REPORT\n" + ModelReport());
            EditorApplication.Exit(0);
        }

        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll) return;
            if (!EditorApplication.isPlaying)
            {
                // Outside Play mode only "play" is honoured (after a script reload).
                if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (!File.Exists(CommandFile)) return;
                string idle = File.ReadAllText(CommandFile).Trim();
                if (idle == "play")
                {
                    File.Delete(CommandFile);
                    PlayFromBootstrap();
                }
                else if (idle.StartsWith("editstatic "))
                {
                    // e.g. "editstatic SceneBuilder BuildAll": runs an editor method
                    // in this editor (one Unity instance per project).
                    File.Delete(CommandFile);
                    string result;
                    try { result = CallStatic(("static " + idle.Substring(11)).Split(' ')); }
                    catch (Exception e) { result = "ERROR " + (e.InnerException ?? e).Message; }
                    File.AppendAllText(LogFile, "> " + idle + "\n" + result + "\nDONE\n");
                }
                return;
            }
            nextPoll = EditorApplication.timeSinceStartup + 0.2;
            if (!File.Exists(CommandFile)) return;
            string[] lines;
            try
            {
                lines = File.ReadAllLines(CommandFile);
                File.Delete(CommandFile);
            }
            catch (IOException)
            {
                return; // writer still holds the file; next poll
            }
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string result;
                try
                {
                    result = Run(line.Trim());
                }
                catch (Exception e)
                {
                    result = "ERROR " + e.GetType().Name + ": " + (e.InnerException ?? e).Message;
                }
                File.AppendAllText(LogFile, "> " + line.Trim() + "\n" + result + "\n");
            }
        }

        static string Run(string line)
        {
            string[] a = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            switch (a[0].ToLowerInvariant())
            {
                case "shot":
                    Directory.CreateDirectory(ShotFolder);
                    string path = ShotFolder + "/" + a[1] + ".png";
                    ScreenCapture.CaptureScreenshot(path);
                    return "shot " + path;
                case "tp": return Teleport(a);
                case "scene":
                    SceneManager.LoadScene(a[1]);
                    return "loading " + a[1];
                case "interact": return InteractWith(a[1]);
                case "static": return CallStatic(a);
                case "invoke": return InvokeOn(a);
                case "get": return GetMember(a);
                case "set": return SetMember(a);
                case "diag": return Diagnostics();
                case "lights": return Lights();
                case "find": return FindNames(a[1]);
                case "models": return ModelReport();
                case "ambient": return AmbientReport();
                case "cfg": return ConfigField(a);
                case "ui": return UiReport();
                case "darkprobe":
                    // Experiment: a flat SH probe from the scene's ambient colour and
                    // no environment reflections, to see what the stale probe adds.
                    var sh = new UnityEngine.Rendering.SphericalHarmonicsL2();
                    sh.AddAmbientLight(RenderSettings.ambientLight);
                    RenderSettings.ambientProbe = sh;
                    RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                    RenderSettings.customReflectionTexture = null;
                    RenderSettings.reflectionIntensity = 0f;
                    return AmbientReport();
                case "lightsoff":
                    foreach (Light l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
                        if (l.type == LightType.Point) l.enabled = false;
                    return "point lights off";
                case "timescale":
                    Time.timeScale = F(a[1]);
                    return "timeScale " + Time.timeScale;
                case "stop":
                    EditorApplication.ExitPlaymode();
                    return "stopping";
                default: return "unknown command";
            }
        }

        static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

        static GameObject Find(string name)
        {
            GameObject direct = GameObject.Find(name);
            if (direct != null) return direct;
            foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name == name) return t.gameObject;
            }
            return null;
        }

        static string Teleport(string[] a)
        {
            var player = UnityEngine.Object.FindAnyObjectByType<LightShadow.Player.PlayerController>();
            if (player == null) return "no player";
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(F(a[1]), F(a[2]), F(a[3]));
            if (a.Length > 4) player.transform.rotation = Quaternion.Euler(0f, F(a[4]), 0f);
            if (a.Length > 5)
            {
                FieldInfo pitch = typeof(LightShadow.Player.PlayerController).GetField("pitch",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (pitch != null) pitch.SetValue(player, F(a[5]));
            }
            if (cc != null) cc.enabled = true;
            return "player at " + player.transform.position;
        }

        static string InteractWith(string name)
        {
            GameObject go = Find(name);
            if (go == null) return "not found";
            var player = UnityEngine.Object.FindAnyObjectByType<LightShadow.Player.PlayerController>();
            foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb is LightShadow.Gameplay.IInteractable i)
                {
                    i.Interact(player != null ? player.gameObject : null);
                    return "interacted " + mb.GetType().Name;
                }
            }
            return "no IInteractable on " + name;
        }

        static Type FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in SafeTypes(asm))
                {
                    if (t.Name == name || t.FullName == name) return t;
                }
            }
            return null;
        }

        static Type[] SafeTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null).ToArray(); }
        }

        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        static object[] Args(MethodInfo m, string[] raw)
        {
            ParameterInfo[] ps = m.GetParameters();
            var values = new object[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                if (i >= raw.Length)
                {
                    values[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue : null;
                    continue;
                }
                values[i] = Convert(raw[i], ps[i].ParameterType);
            }
            return values;
        }

        static object Convert(string raw, Type type)
        {
            if (type == typeof(string)) return raw;
            if (type == typeof(float)) return F(raw);
            if (type == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(bool)) return bool.Parse(raw);
            if (type.IsEnum) return Enum.Parse(type, raw, true);
            if (type == typeof(GameObject)) return Find(raw);
            return null;
        }

        static MethodInfo Method(Type t, string name, int argc)
        {
            return t.GetMethods(All).FirstOrDefault(m => m.Name == name && m.GetParameters().Length >= argc
                && m.GetParameters().Skip(argc).All(p => p.HasDefaultValue))
                ?? t.GetMethods(All).FirstOrDefault(m => m.Name == name);
        }

        static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string s) return "\"" + s + "\"";
            if (value is System.Collections.IEnumerable e && !(value is UnityEngine.Object))
            {
                var sb = new StringBuilder("[");
                foreach (object o in e) sb.Append(Describe(o)).Append(", ");
                return sb.Append("]").ToString();
            }
            return value.ToString();
        }

        static string CallStatic(string[] a)
        {
            Type t = FindType(a[1]);
            if (t == null) return "no type " + a[1];
            string[] rest = a.Skip(3).ToArray();
            MethodInfo m = Method(t, a[2], rest.Length);
            if (m != null) return Describe(m.Invoke(null, Args(m, rest)));
            PropertyInfo p = t.GetProperty(a[2], All);
            if (p != null) return Describe(p.GetValue(null));
            FieldInfo f = t.GetField(a[2], All);
            if (f != null) return Describe(f.GetValue(null));
            return "no member " + a[2];
        }

        static Component ComponentOn(string objectName, string componentName, out string error)
        {
            error = null;
            GameObject go = Find(objectName);
            if (go == null) { error = "no object " + objectName; return null; }
            Component c = go.GetComponents<Component>().FirstOrDefault(x => x != null && x.GetType().Name == componentName)
                ?? go.GetComponentsInChildren<Component>(true).FirstOrDefault(x => x != null && x.GetType().Name == componentName);
            if (c == null) error = "no component " + componentName + " on " + objectName;
            return c;
        }

        static string InvokeOn(string[] a)
        {
            Component c = ComponentOn(a[1], a[2], out string error);
            if (c == null) return error;
            string[] rest = a.Skip(4).ToArray();
            MethodInfo m = Method(c.GetType(), a[3], rest.Length);
            if (m == null) return "no method " + a[3];
            return Describe(m.Invoke(m.IsStatic ? null : c, Args(m, rest)));
        }

        static string GetMember(string[] a)
        {
            Component c = ComponentOn(a[1], a[2], out string error);
            if (c == null) return error;
            PropertyInfo p = c.GetType().GetProperty(a[3], All);
            if (p != null) return Describe(p.GetValue(c));
            FieldInfo f = c.GetType().GetField(a[3], All);
            if (f != null) return Describe(f.GetValue(c));
            return "no member " + a[3];
        }

        static string SetMember(string[] a)
        {
            Component c = ComponentOn(a[1], a[2], out string error);
            if (c == null) return error;
            PropertyInfo p = c.GetType().GetProperty(a[3], All);
            if (p != null && p.CanWrite) { p.SetValue(c, Convert(a[4], p.PropertyType)); return "ok"; }
            FieldInfo f = c.GetType().GetField(a[3], All);
            if (f != null) { f.SetValue(c, Convert(a[4], f.FieldType)); return "ok"; }
            return "no writable member " + a[3];
        }

        static string Diagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("scene " + SceneManager.GetActiveScene().name + " t=" + Time.time.ToString("0.0")
                + " timeScale " + Time.timeScale + " fps~" + (1f / Mathf.Max(0.0001f, Time.smoothDeltaTime)).ToString("0"));
            sb.AppendLine("cursor lock " + Cursor.lockState + " visible " + Cursor.visible);
            var player = UnityEngine.Object.FindAnyObjectByType<LightShadow.Player.PlayerController>();
            if (player != null)
            {
                sb.AppendLine("player " + player.transform.position + " yaw " + player.transform.eulerAngles.y.ToString("0")
                    + " inputLocked " + LightShadow.Player.PlayerController.ExternalInputLocked);
            }
            var enemy = UnityEngine.Object.FindAnyObjectByType<LightShadow.Enemy.EnemyBehaviour>();
            if (enemy != null)
            {
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                sb.AppendLine("enemy " + enemy.transform.position + " state " + Describe(enemy.CurrentState)
                    + (agent != null ? " agent enabled " + agent.enabled + " onMesh " + (agent.enabled && agent.isOnNavMesh)
                        + " vel " + agent.velocity.magnitude.ToString("0.00") : ""));
            }
            sb.AppendLine("ambient " + RenderSettings.ambientMode + " " + RenderSettings.ambientLight
                + " sky " + RenderSettings.ambientSkyColor + " fog " + RenderSettings.fog + " " + RenderSettings.fogColor
                + " density " + RenderSettings.fogDensity + " skybox " + (RenderSettings.skybox != null ? RenderSettings.skybox.name : "none"));
            sb.AppendLine("listener volume " + AudioListener.volume + " paused " + AudioListener.pause);
            int playing = 0;
            foreach (AudioSource s in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude))
            {
                if (!s.isPlaying) continue;
                playing++;
                sb.AppendLine("  audio " + s.name + " clip " + (s.clip != null ? s.clip.name : "-") + " vol " + s.volume.ToString("0.00")
                    + " group " + (s.outputAudioMixerGroup != null ? s.outputAudioMixerGroup.name : "-") + " mute " + s.mute);
            }
            sb.AppendLine("playing sources " + playing);
            return sb.ToString();
        }

        static string Lights()
        {
            var sb = new StringBuilder();
            int count = 0;
            foreach (Light l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (!l.enabled) continue;
                count++;
                sb.AppendLine(l.type + " " + Path(l.transform) + " i=" + l.intensity.ToString("0.00") + " r=" + l.range.ToString("0.0")
                    + " shadows " + l.shadows + (l.type == LightType.Spot ? " angle " + l.spotAngle.ToString("0") : ""));
            }
            return count + " active lights\n" + sb;
        }

        static string FindNames(string part)
        {
            var sb = new StringBuilder();
            foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                    sb.AppendLine(Path(t) + " @ " + t.position);
            }
            return sb.ToString();
        }

        // What Unity's OBJ importer really makes of the delivered player models.
        public static string ModelReport()
        {
            var sb = new StringBuilder();
            foreach (string model in new[] { "SM_Torch", "SM_Player_Hand_R", "SM_Player_Hand_L", "SM_Player_Body" })
            {
                string assetPath = "Assets/_Project/Models/Hero/" + model + ".obj";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) { sb.AppendLine(model + ": NOT IMPORTED"); continue; }
                // Renderer bounds are only valid on a scene instance.
                GameObject asset = UnityEngine.Object.Instantiate(prefab, new Vector3(0f, -500f, 0f), Quaternion.identity);
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                sb.AppendLine(model + ": root rot " + asset.transform.localEulerAngles + " scale " + asset.transform.localScale
                    + (importer != null ? " globalScale " + importer.globalScale + " useFileScale " + importer.useFileScale
                        + " normals " + importer.importNormals + " bakeAxis " + importer.bakeAxisConversion : ""));
                foreach (Transform t in asset.GetComponentsInChildren<Transform>(true))
                {
                    if (t == asset.transform) continue;
                    MeshFilter mf = t.GetComponent<MeshFilter>();
                    Renderer r = t.GetComponent<Renderer>();
                    Vector3 c = r != null ? asset.transform.InverseTransformPoint(r.bounds.center) : Vector3.zero;
                    Mesh mesh = mf != null ? mf.sharedMesh : null;
                    sb.AppendLine("  " + t.name + " localPos " + t.localPosition.ToString("F4") + " localRot " + t.localEulerAngles.ToString("F1")
                        + " boundsCentre " + c.ToString("F4") + " size " + (r != null ? r.bounds.size.ToString("F4") : "-")
                        + (mesh != null ? " verts " + mesh.vertexCount + " normals " + mesh.normals.Length + " uv " + mesh.uv.Length : ""));
                }
                UnityEngine.Object.DestroyImmediate(asset);
            }
            return sb.ToString();
        }

        static string ConfigField(string[] a)
        {
            var gm = LightShadow.Core.GameManager.Instance;
            if (gm == null || gm.config == null) return "no GameConfig";
            FieldInfo f = gm.config.GetType().GetField(a[1], All);
            if (f == null) return "no field " + a[1];
            if (a.Length > 2) f.SetValue(gm.config, Convert(a[2], f.FieldType));
            var meter = UnityEngine.Object.FindAnyObjectByType<LightShadow.Player.BrightIdeaMeter>();
            if (meter != null) meter.ApplyToLight();
            return a[1] + " = " + Describe(f.GetValue(gm.config));
        }

        static string UiReport()
        {
            var sb = new StringBuilder();
            foreach (UnityEngine.UI.Graphic g in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsInactive.Exclude))
            {
                if (!g.enabled || g.canvasRenderer.cull) continue;
                float alpha = g.color.a * g.canvasRenderer.GetInheritedAlpha();
                if (alpha < 0.02f) continue;
                Vector3[] c = new Vector3[4];
                g.rectTransform.GetWorldCorners(c);
                string text = g is UnityEngine.UI.Text t ? " text \"" + t.text.Replace('\n', ' ') + "\"" : "";
                sb.AppendLine(Path(g.transform) + " [" + g.GetType().Name + "] rect " + c[0].ToString("F0") + "-" + c[2].ToString("F0")
                    + " colour " + g.color + " alpha " + alpha.ToString("0.00") + text);
            }
            return sb.ToString();
        }

        static string AmbientReport()
        {
            var sh = RenderSettings.ambientProbe;
            var dirs = new[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.right };
            var colors = new Color[dirs.Length];
            sh.Evaluate(dirs, colors);
            var sb = new StringBuilder("ambientProbe up " + colors[0] + " down " + colors[1] + " fwd " + colors[2] + " right " + colors[3]);
            sb.Append(" | reflection " + RenderSettings.defaultReflectionMode + " x" + RenderSettings.reflectionIntensity
                + " custom " + (RenderSettings.customReflectionTexture != null ? RenderSettings.customReflectionTexture.name : "none")
                + " | lightmapsData " + (Lightmapping.lightingDataAsset != null ? Lightmapping.lightingDataAsset.name : "none"));
            return sb.ToString();
        }

        static string Path(Transform t)
        {
            string p = t.name;
            for (Transform x = t.parent; x != null; x = x.parent) p = x.name + "/" + p;
            return p;
        }
    }
}
